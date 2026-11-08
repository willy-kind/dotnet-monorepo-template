using System.Net;
using System.Net.Http.Json;

using Example.Api.Features.Orders;

using Microsoft.AspNetCore.Mvc;

namespace Example.Api.Tests.Features.Orders;

public class CreateOrderTests
{
    private static readonly object ValidOrder = new { customerId = "c-1", lines = new[] { new { sku = "sku-1", quantity = 2 } } };

    [Fact]
    public async Task CreatesTheOrderInErp()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = new TestApp();
        app.Erp.Respond(HttpStatusCode.OK, new { orderId = "o-1" });
        using var client = app.CreateClient();

        using var response = await client.PostAsJsonAsync(new Uri("/orders", UriKind.Relative), ValidOrder, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(new CreateOrder.Response("o-1"), await response.Content.ReadFromJsonAsync<CreateOrder.Response>(ct));
        var sent = Assert.Single(app.Erp.Requests);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal(new Uri("https://erp.example.com/orders"), sent.Uri);
        Assert.Equal("""{"customerId":"c-1","lines":[{"sku":"sku-1","quantity":2}]}""", sent.Body);
    }

    [Fact]
    public async Task RejectsAnInvalidOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = new TestApp();
        using var client = app.CreateClient();

        using var response = await client.PostAsJsonAsync(new Uri("/orders", UriKind.Relative), new { customerId = " ", lines = new[] { new { sku = "sku-1", quantity = 0 } } }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);
        Assert.Equal(["customerId", "lines"], problem!.Errors.Keys.Order());
        Assert.Empty(app.Erp.Requests);
    }

    [Theory]
    [InlineData("server error")]
    [InlineData("timeout")]
    public async Task ReturnsBadGatewayWhenErpFails(string failure)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = new TestApp();
        if (failure == "timeout")
        {
            app.Erp.Throw(new TaskCanceledException());
        }
        else
        {
            app.Erp.Respond(HttpStatusCode.InternalServerError);
        }

        using var client = app.CreateClient();

        using var response = await client.PostAsJsonAsync(new Uri("/orders", UriKind.Relative), ValidOrder, ct);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        Assert.Equal("ERP is unavailable.", problem!.Title);
    }
}
