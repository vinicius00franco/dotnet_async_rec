using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_async.Client;
public class JornadaMilhasClientFactory : IHttpClientFactory
{
    private string url = "http://localhost:5125";
    public HttpClient CreateClient(string name)
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            ); 

        return client;
    }
}
