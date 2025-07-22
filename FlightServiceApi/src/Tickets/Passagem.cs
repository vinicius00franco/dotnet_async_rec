namespace FlightServiceApi.src.Tickets;

public class Passagem
{
    public int Id { get; set; }
    public int VooId { get; set; } // Referência ao voo
    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public int MilhasUtilizadas { get; set; }
    public DateTime DataCompra { get; set; }
    public string Status { get; set; } = "Confirmada";
}
