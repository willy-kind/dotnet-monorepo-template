using System.Net.Http.Json;

namespace Example.Job.Integrations.Erp;

public sealed class ErpClient(HttpClient http)
{
    public async Task UpsertCustomerAsync(ErpCustomer customer, CancellationToken ct)
    {
        using var response = await http.PutAsJsonAsync(new Uri($"customers/{Uri.EscapeDataString(customer.Id)}", UriKind.Relative), customer, ct);
        response.EnsureSuccessStatusCode();
    }
}

public sealed record ErpCustomer(string Id, string Name, string Email);
