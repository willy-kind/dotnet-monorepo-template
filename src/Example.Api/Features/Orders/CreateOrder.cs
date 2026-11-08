using Example.Api.Integrations.Erp;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Example.Api.Features.Orders;

public static partial class CreateOrder
{
    public static void Map(IEndpointRouteBuilder app) => app.MapPost("/orders", HandleAsync);

    internal static async Task<Results<Created<Response>, ValidationProblem, ProblemHttpResult>> HandleAsync(
        Request request, ErpClient erp, CancellationToken ct)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var order = new ErpCreateOrder(
            request.CustomerId!,
            [.. request.Lines!.Select(line => new ErpOrderLine(line.Sku!, line.Quantity))]);

        try
        {
            var created = await erp.CreateOrderAsync(order, ct);
            return TypedResults.Created((string?)null, new Response(created.OrderId));
        }
        catch (HttpRequestException)
        {
            return ErpUnavailable();
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return ErpUnavailable();
        }
    }

    private static Dictionary<string, string[]> Validate(Request request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            errors["customerId"] = ["Customer ID is required."];
        }

        if (request.Lines is null or [])
        {
            errors["lines"] = ["At least one line is required."];
        }
        else if (request.Lines.Any(line => string.IsNullOrWhiteSpace(line.Sku) || line.Quantity <= 0))
        {
            errors["lines"] = ["Every line needs a SKU and a quantity above 0."];
        }

        return errors;
    }

    private static ProblemHttpResult ErpUnavailable() => TypedResults.Problem(title: "ERP is unavailable.", statusCode: StatusCodes.Status502BadGateway);
}
