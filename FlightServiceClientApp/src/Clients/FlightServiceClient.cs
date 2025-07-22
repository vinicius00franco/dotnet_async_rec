using FlightServiceClientApp.src.Models;
using System.Net.Http.Json;

namespace FlightServiceClientApp.src.Clients;

// Definindo os DTOs que o cliente espera receber
public record VooDetalhadoDto(int Id, string Rota, int Milhas, int Assentos, string Status);
public record RelatorioStatus(Guid Id, string Status, string? UrlDownload, string? Erro);

public class FlightServiceClient
{
    private readonly HttpClient _client;

    public FlightServiceClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<VooDetalhadoDto?> GetFlightDetailsAsync(int id)
    {
        try
        {
            return await _client.GetFromJsonAsync<VooDetalhadoDto>($"/voos/{id}");
        }
        catch (HttpRequestException)
        {
            return null; // Retorna nulo se o voo não for encontrado (404)
        }
    }

    public async Task<string> PurchaseTicketAsync(int vooId, CompraPassagemRequest request)
    {
        var response = await _client.PostAsJsonAsync($"/voos/{vooId}/comprar", request);
        var content = await response.Content.ReadAsStringAsync();
        return $"Status: {response.StatusCode}, Resposta: {content}";
    }

    public async Task<string?> RequestReportAsync()
    {
        var response = await _client.PostAsync("/relatorios/voos", null);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            // Retorna a URL de status do header "Location"
            return response.Headers.Location?.OriginalString;
        }
        return null;
    }

    public async Task<RelatorioStatus?> GetReportStatusAsync(string statusUrl)
    {
        return await _client.GetFromJsonAsync<RelatorioStatus>(statusUrl);
    }

    public async Task DownloadReportAsync(string downloadUrl, string savePath)
    {
        var fileBytes = await _client.GetByteArrayAsync(downloadUrl);
        await File.WriteAllBytesAsync(savePath, fileBytes);
    }
}
