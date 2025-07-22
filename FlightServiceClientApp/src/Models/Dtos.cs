namespace FlightServiceClientApp.src.Models;

// DTO para receber os detalhes de um voo da API
public record VooDetalhadoDto(int Id, string Rota, int Milhas, int Assentos, string Status);

// DTO para enviar uma requisição de compra de passagem
public record CompraPassagemRequest(string Origem, string Destino, int Milhas);

// DTO para receber o status de um relatório
public record RelatorioStatus(Guid Id, string Status, string? UrlDownload, string? Erro);
