using FlightServiceClientApp.src.Flights;
using FlightServiceClientApp.src.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FlightServiceClientApp.src.Flights;
public class FlightServiceClient
{
    private HttpClient client;
    public FlightServiceClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<IEnumerable<Voo>> GetFlightsAsync(CancellationToken token=default)
    {
        HttpResponseMessage response = await client.GetAsync("/Voos",token);
        return await response.Content.ReadFromJsonAsync<IEnumerable<Voo>>() ?? Enumerable.Empty<Voo>();
    }

    public async Task<string> PurchaseTicketAsync(CompraPassagemRequest request)
    {
        return await client.PostAsJsonAsync("/Voos/comprar", request).Result.Content.ReadFromJsonAsync<string>() ?? string.Empty;
    }
}
