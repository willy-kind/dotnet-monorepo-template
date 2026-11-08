using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Example.Api.Tests;

/// <summary>
/// The real API, in memory. Each integration gets one <see cref="Testing.FakeHttpHandler"/> property
/// here, swapped in with <c>ConfigureTestServices</c>:
/// <c>services.AddHttpClient&lt;CrmClient&gt;().ConfigurePrimaryHttpMessageHandler(() => Crm);</c>
/// </summary>
public sealed class TestApp : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
}
