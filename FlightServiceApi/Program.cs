using FlightServiceApi.src.Context;
using FlightServiceApi.src.Flights;
using FlightServiceApi.src.Reports;
using FlightServiceApi.src.Services;
using FlightServiceApi.src.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- Injeção de Dependência ---
builder.Services.AddDbContext<JornadaMilhasContext>(options =>
    options.UseInMemoryDatabase("JornadaMilhasDB_V2"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("RedisConnectionString");
    options.InstanceName = "FlightService_";
});

builder.Services.AddHttpClient("MilesService", client =>
{
    // Em um cenário real, a URL viria do appsettings
    client.BaseAddress = new Uri("http://localhost:5125");
});

builder.Services.AddSingleton<RelatorioService>(); // Singleton para manter o estado dos jobs

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Carga de Dados Inicial ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JornadaMilhasContext>();
    context.Database.EnsureCreated();
    if (!context.Voos.Any())
    {
        context.Voos.AddRange(
            new Voo { Id = 1, Origem = "Brasilia", Destino = "Recife", Preco = 2000, MilhasNecessarias = 10000, AssentosDisponiveis = 10 },
            new Voo { Id = 2, Origem = "Vitória", Destino = "São Paulo", Preco = 2500, MilhasNecessarias = 15000, AssentosDisponiveis = 5 },
            new Voo { Id = 3, Origem = "Salvador", Destino = "Florianópolis", Preco = 3000, MilhasNecessarias = 20000, AssentosDisponiveis = 0 } // Voo lotado
        );
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Endpoints Aprimorados ---

#region Voos e Cache com DTO
app.MapGet("/voos/{id}", async (int id, [FromServices] JornadaMilhasContext context, [FromServices] IDistributedCache cache) =>
{
    string cacheKey = $"voo_detalhado:{id}";
    string? cachedVoo = await cache.GetStringAsync(cacheKey);

    if (!string.IsNullOrEmpty(cachedVoo))
    {
        Console.WriteLine(">>> DTO do Voo encontrado no cache! <<<");
        return Results.Ok(JsonSerializer.Deserialize<VooDetalhadoDto>(cachedVoo));
    }

    var voo = await context.Voos.FindAsync(id);
    if (voo is null) return Results.NotFound();

    // Transforma a entidade em DTO
    var vooDto = new VooDetalhadoDto(voo.Id, $"{voo.Origem}-{voo.Destino}", voo.MilhasNecessarias, voo.AssentosDisponiveis, voo.Status.ToString());

    var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
    await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(vooDto), options);

    return Results.Ok(vooDto);
})
.WithTags("Voos").WithSummary("Obtém um voo detalhado (com cache de DTO).");
#endregion

#region Compra com Regras de Negócio
// Serviço externo simulado
app.MapPost("/servico-milhas/validar", async ([FromBody] CompraPassagemRequest request) =>
{
    await Task.Delay(600); // Simula latência de rede
    if (request.Milhas < 10000)
        return Results.BadRequest(new { Aprovado = false, Motivo = "Quantidade de milhas insuficiente." });
    
    return Results.Ok(new { Aprovado = true, TransacaoId = Guid.NewGuid() });
}).WithTags("Serviço Externo (Simulado)");

app.MapPost("/voos/{vooId}/comprar", async (int vooId, [FromBody] CompraPassagemRequest request, [FromServices] JornadaMilhasContext context, [FromServices] IHttpClientFactory factory) =>
{
    var voo = await context.Voos.FindAsync(vooId);
    if (voo is null) return Results.NotFound("Voo não encontrado.");
    
    // Regra de negócio 1: Verificar assentos
    if (voo.AssentosDisponiveis <= 0) return Results.BadRequest("Voo lotado. Não há assentos disponíveis.");
    
    // Regra de negócio 2: Chamar serviço externo de milhas
    var milesClient = factory.CreateClient("MilesService");
    var response = await milesClient.PostAsJsonAsync("/servico-milhas/validar", request);
    
    if (!response.IsSuccessStatusCode)
    {
        var erro = await response.Content.ReadFromJsonAsync<object>();
        return Results.Problem(detail: $"Falha na validação de milhas: {erro}", statusCode: StatusCodes.Status422UnprocessableEntity);
    }
    
    // Regra de negócio 3: Decrementar assento e salvar passagem (operação de escrita)
    voo.AssentosDisponiveis--;
    
    var passagem = new Passagem
    {
        VooId = vooId,
        Origem = voo.Origem,
        Destino = voo.Destino,
        MilhasUtilizadas = request.Milhas,
        DataCompra = DateTime.UtcNow
    };
    
    context.Passagens.Add(passagem);
    await context.SaveChangesAsync(); // Persiste ambas as alterações (voo e passagem)
    
    return Results.Ok(new { Mensagem = "Compra realizada com sucesso!", PassagemId = passagem.Id });
})
.WithTags("Voos").WithSummary("Compra uma passagem com validação de assentos e milhas.");
#endregion

#region Geração de Relatórios Assíncrona
app.MapPost("/relatorios/voos", (RelatorioService relatorioService) =>
{
    var relatorio = relatorioService.SolicitarRelatorioDeVoos();
    // Retorna 202 Accepted com a localização para consultar o status
    return Results.Accepted($"/relatorios/{relatorio.Id}");
})
.WithTags("Relatórios").WithSummary("Solicita a geração de um relatório de voos.");

app.MapGet("/relatorios/{id}", (Guid id, RelatorioService relatorioService) =>
{
    var status = relatorioService.GetStatus(id);
    return status is null ? Results.NotFound() : Results.Ok(status);
})
.WithTags("Relatórios").WithSummary("Verifica o status da geração de um relatório.");

app.MapGet("/relatorios/{id}/download", (Guid id, RelatorioService relatorioService) =>
{
    var relatorio = relatorioService.GetStatus(id);
    if (relatorio is null || relatorio.Status != StatusRelatorio.Concluido)
    {
        return Results.NotFound("Relatório não encontrado ou ainda não concluído.");
    }
    
    var filePath = Path.Combine("Reports", $"{id}.csv");
    if (!File.Exists(filePath))
    {
        return Results.NotFound("Arquivo do relatório não encontrado.");
    }
    
    return Results.File(filePath, "text/csv", $"relatorio_voos_{id}.csv");
})
.WithTags("Relatórios").WithSummary("Baixa o relatório concluído.");
#endregion

app.Run();
