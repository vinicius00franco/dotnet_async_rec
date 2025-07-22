using FlightServiceApi.src.Context;
using FlightServiceApi.src.Reports;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FlightServiceApi.src.Services;

public class RelatorioService
{
    private static readonly Dictionary<Guid, Relatorio> _jobs = new();
    private readonly IServiceScopeFactory _scopeFactory;

    public RelatorioService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Relatorio SolicitarRelatorioDeVoos()
    {
        var relatorio = new Relatorio();
        _jobs[relatorio.Id] = relatorio;

        Task.Run(() => GerarRelatorio(relatorio.Id));

        return relatorio;
    }

    public Relatorio? GetStatus(Guid id)
    {
        _jobs.TryGetValue(id, out var relatorio);
        return relatorio;
    }

    private async Task GerarRelatorio(Guid id)
    {
        var relatorio = _jobs[id];
        relatorio.Status = StatusRelatorio.Processando;

        try
        {
            await Task.Delay(5000);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<JornadaMilhasContext>();
            var voos = await context.Voos.AsNoTracking().ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id;Origem;Destino;AssentosDisponiveis;Status");
            foreach (var voo in voos)
            {
                builder.AppendLine($"{voo.Id};{voo.Origem};{voo.Destino};{voo.AssentosDisponiveis};{voo.Status}");
            }

            var reportPath = Path.Combine("Reports", $"{id}.csv");
            Directory.CreateDirectory("Reports");
            await File.WriteAllTextAsync(reportPath, builder.ToString());

            relatorio.Status = StatusRelatorio.Concluido;
            relatorio.ConcluidoEm = DateTime.UtcNow;
            relatorio.UrlDownload = $"/relatorios/{id}/download";
        }
        catch (Exception ex)
        {
            relatorio.Status = StatusRelatorio.Falha;
            relatorio.Erro = ex.Message;
        }
    }
}
