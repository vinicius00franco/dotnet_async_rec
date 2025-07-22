// Original content of CompraPassagemRequest.cs moved here.
namespace FlightServiceClientApp.src.Tickets;

public record CompraPassagemRequest
{
    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public int Milhas { get; set; }
}
