using System.Net.Http.Json;

namespace Example.Job.Integrations.Crm;

public sealed class CrmClient(HttpClient http)
{
    public async Task<IReadOnlyList<CrmCustomer>> GetCustomersAsync(CancellationToken ct) => await http.GetFromJsonAsync<List<CrmCustomer>>(new Uri("customers", UriKind.Relative), ct) ?? [];
}

public sealed record CrmCustomer(string Id, string Name, string Email);
