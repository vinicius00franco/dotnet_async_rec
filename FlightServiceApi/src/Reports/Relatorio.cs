namespace FlightServiceApi.src.Reports;

public enum StatusRelatorio
{
    Pendente,
    Processando,
    Concluido,
    Falha
}

public class Relatorio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime SolicitadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ConcluidoEm { get; set; }
    public StatusRelatorio Status { get; set; } = StatusRelatorio.Pendente;
    public string? UrlDownload { get; set; }
    public string? Erro { get; set; }
}
