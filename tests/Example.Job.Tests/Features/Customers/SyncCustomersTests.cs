using System.Net;

using Example.Job.Features.Customers;

namespace Example.Job.Tests.Features.Customers;

public class SyncCustomersTests
{
    [Fact]
    public async Task UpsertsEveryCrmCustomerIntoErp()
    {
        await using var app = new TestApp();
        app.Crm.Respond(HttpStatusCode.OK, new[]
        {
            new { id = "c-1", name = "Ada", email = "ada@example.com" },
            new { id = "c-2", name = "Linus", email = "linus@example.com" },
        });
        app.Erp.Respond(HttpStatusCode.NoContent);

        await app.Get<SyncCustomers.Job>().RunOnceAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [new Uri("https://erp.test/customers/c-1"), new Uri("https://erp.test/customers/c-2")],
            app.Erp.Requests.Select(request => request.Uri));
        Assert.All(app.Erp.Requests, request => Assert.Equal(HttpMethod.Put, request.Method));
        Assert.Equal("""{"id":"c-1","name":"Ada","email":"ada@example.com"}""", app.Erp.Requests[0].Body);
    }

    [Fact]
    public async Task FailsWhenCrmIsUnavailable()
    {
        await using var app = new TestApp();
        app.Crm.Respond(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() => app.Get<SyncCustomers.Job>().RunOnceAsync(TestContext.Current.CancellationToken));

        Assert.Empty(app.Erp.Requests);
    }
}
