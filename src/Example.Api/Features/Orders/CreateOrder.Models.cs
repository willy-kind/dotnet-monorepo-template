namespace Example.Api.Features.Orders;

public static partial class CreateOrder
{
    public sealed record Request(string? CustomerId, IReadOnlyList<RequestLine>? Lines);

    public sealed record RequestLine(string? Sku, int Quantity);

    public sealed record Response(string OrderId);
}
