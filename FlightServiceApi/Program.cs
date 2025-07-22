using FlightServiceApi.src.Context;
using FlightServiceApi.src.Flights;
using FlightServiceApi.src.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<JornadaMilhasContext>(options => options.UseInMemoryDatabase("JornadaMilhasVoos"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JornadaMilhasContext>();

    // Carga de dados inicial
    context.Voos.AddRange(
        new Voo { Id = 1, Origem = "Brasilia", Destino = "Recife", Preco = 2000, MilhasNecessarias = 10000 },
            new Voo { Id = 2, Origem = "Vit�ria", Destino = "S�o Paulo", Preco = 2500, MilhasNecessarias = 15000 },
            new Voo { Id = 3, Origem = "Salvador", Destino = "Florian�polis", Preco = 3000, MilhasNecessarias = 20000 }
    );
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/Hello", async () => {

    await Task.FromResult("Hello World! - API online.");
    
    }).WithTags("Voos").WithSummary("Verifica��o do status 'Online'").WithOpenApi();

app.MapGet("/voos", async ([FromServices]JornadaMilhasContext context, CancellationToken token = default) => {
    try
    {
        token.ThrowIfCancellationRequested();
        var voos = await context.Voos.ToListAsync(token);
        return Results.Ok(voos);
    }
    catch (OperationCanceledException ex)
    {

        return Results.Problem($"Opera��o cancelada: {ex.Message}");
    }   

}).WithTags("Voos").WithSummary("Lista os v�os cadastrados.").WithOpenApi();

app.MapGet("/voos/{id}", async ([FromServices] JornadaMilhasContext context, int id) => {

    var voo = await context.Voos.FindAsync(id);
    if (voo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(voo);

});

app.MapPost("/voos/comprar",async([FromServices]JornadaMilhasContext context, [FromBody] CompraPassagemRequest request) =>
{
    var mensagemCompra = $"Passagem comprada origem: {request.Origem} com destino: {request.Destino} com milhas {request.Milhas}";
    return Results.Ok(await Task.FromResult(mensagemCompra));

}).WithTags("Voos").WithSummary("Simula a compra de passagem").WithOpenApi();

app.Run();