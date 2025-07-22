namespace dotnet_async.Modelos;

public class Voo
{
    public int Id { get; set; }
    public string? Origem { get; set; }
    public string? Destino { get; set; }
    public int Preco { get; set; }
    public int MilhasNecessarias { get; set; }

    public override string ToString()
    {
        return $"Voo {Id} - Origem: {Origem}, Destino: {Destino}, Preço: {Preco}, Milhas Necessárias: {MilhasNecessarias}";
    }
}
