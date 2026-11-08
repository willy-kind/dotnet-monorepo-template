using Example.Api.Integrations.Erp;
using Example.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Api.Tests;

/// <summary>
/// The real API, in memory, with each integration's HTTP calls going to a <see cref="FakeHttpHandler"/>.
/// A new integration gets one property here and one line in <see cref="ConfigureWebHost"/>.
/// </summary>
public sealed class TestApp : WebApplicationFactory<Program>
{
    public FakeHttpHandler Erp { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services => services.AddHttpClient<ErpClient>().ConfigurePrimaryHttpMessageHandler(() => Erp));
    }
}
