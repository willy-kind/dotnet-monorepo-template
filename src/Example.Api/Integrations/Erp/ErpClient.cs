namespace Example.Api.Integrations.Erp;

public sealed class ErpClient(HttpClient http)
{
    public async Task<ErpOrderCreated> CreateOrderAsync(ErpCreateOrder order, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync(new Uri("orders", UriKind.Relative), order, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ErpOrderCreated>(ct)
            ?? throw new InvalidOperationException("ERP returned an empty body for a created order.");
    }
}

public sealed record ErpCreateOrder(string CustomerId, IReadOnlyList<ErpOrderLine> Lines);

public sealed record ErpOrderLine(string Sku, int Quantity);

public sealed record ErpOrderCreated(string OrderId);
