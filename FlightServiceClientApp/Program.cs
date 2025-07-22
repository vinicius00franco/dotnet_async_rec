using FlightServiceClientApp.src.Clients;
using FlightServiceClientApp.src.Factories;
using FlightServiceClientApp.src.Models;
using System;
using System.IO;
using System.Threading.Tasks;

var factory = new FlightServiceHttpClientFactory(); // Corrigido para usar o nome correto da classe
var httpClient = factory.CreateClient();
var client = new FlightServiceClient(httpClient);

Console.WriteLine("--- INICIANDO SIMULAÇÕES AVANÇADAS ---");

// --- Cenário 1: Cache com DTO ---
Console.WriteLine("\n--- Testando Cache com DTO (Voo ID 1) ---");
Console.WriteLine("1ª Chamada (deve vir do DB e criar DTO):");
var dto1 = await client.GetFlightDetailsAsync(1);
Console.WriteLine(dto1 != null ? $"Recebido: {dto1}" : "Voo não encontrado.");

Console.WriteLine("\n2ª Chamada (deve vir do Cache da API):");
var dto2 = await client.GetFlightDetailsAsync(1);
Console.WriteLine(dto2 != null ? $"Recebido: {dto2}" : "Voo não encontrado.");

// --- Cenário 2: Compra com Regras de Negócio ---
Console.WriteLine("\n--- Testando Compra com Regras de Negócio ---");
Console.WriteLine("\nTentativa 1: Comprar passagem em voo lotado (ID 3)...");
var compraLotado = await client.PurchaseTicketAsync(3, new CompraPassagemRequest("Salvador", "Florianópolis", 20000));
Console.WriteLine(compraLotado);

Console.WriteLine("\nTentativa 2: Comprar passagem com milhas insuficientes (Voo ID 1)...");
var compraMilhasInsuficientes = await client.PurchaseTicketAsync(1, new CompraPassagemRequest("Brasilia", "Recife", 5000));
Console.WriteLine(compraMilhasInsuficientes);

Console.WriteLine("\nTentativa 3: Compra com sucesso (Voo ID 2)...");
var compraSucesso = await client.PurchaseTicketAsync(2, new CompraPassagemRequest("Vitória", "São Paulo", 15000));
Console.WriteLine(compraSucesso);

// --- Cenário 3: Geração de Relatório Assíncrona com Polling ---
Console.WriteLine("\n--- Testando Geração de Relatório Assíncrona ---");
string? statusUrl = await client.RequestReportAsync();
if (statusUrl is null)
{
    Console.WriteLine("Falha ao solicitar o relatório.");
    return;
}

Console.WriteLine($"Relatório solicitado. Verificando status em: {statusUrl}");
var status = await client.GetReportStatusAsync(statusUrl); // Corrigido para usar o tipo correto
Console.WriteLine($"Status do relatório ({status?.Id}): {status?.Status}");

if (status?.Status == "Concluido" && status.UrlDownload is not null)
{
    string savePath = "relatorio_final.csv";
    Console.WriteLine($"Relatório concluído! Baixando de {status.UrlDownload}...");
    await client.DownloadReportAsync(status.UrlDownload, savePath);
    Console.WriteLine($"Relatório salvo em: {Path.GetFullPath(savePath)}");
    File.Delete(savePath); // Limpa o arquivo
}
else
{
    Console.WriteLine($"Geração do relatório falhou: {status?.Erro}");
}

Console.WriteLine("\n--- Simulações Concluídas ---");
