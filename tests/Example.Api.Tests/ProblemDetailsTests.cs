using System.Net;

namespace Example.Api.Tests;

public class ProblemDetailsTests
{
    [Fact]
    public async Task UnknownRouteReturnsProblemDetails()
    {
        await using var app = new TestApp();
        using var client = app.CreateClient();

        using var response = await client.GetAsync(new Uri("/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
