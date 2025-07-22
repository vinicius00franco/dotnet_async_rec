namespace FlightServiceClientApp.src.Flights;

public class Voo
{
    public int Id { get; set; }
    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public int Preco { get; set; }
    public int MilhasNecessarias { get; set; }
    public int AssentosDisponiveis { get; set; }
    public string Status { get; set; } = "Programado";
}
