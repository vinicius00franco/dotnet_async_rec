namespace FlightServiceApi.src.Flights;

public enum StatusVoo
{
    Programado,
    Confirmado,
    Atrasado,
    Cancelado
}

public class Voo
{
    public int Id { get; set; }
    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public int Preco { get; set; }
    public int MilhasNecessarias { get; set; }
    public int AssentosDisponiveis { get; set; } // Nova propriedade
    public StatusVoo Status { get; set; } = StatusVoo.Programado; // Nova propriedade

    public override string ToString()
    {
        return $"Voo {Id} - Origem: {Origem}, Destino: {Destino}, Preço: {Preco}, Milhas Necessárias: {MilhasNecessarias}";
    }
}

// DTO para o cache e para a resposta do endpoint de detalhe
public record VooDetalhadoDto(int Id, string Rota, int Milhas, int Assentos, string Status);
