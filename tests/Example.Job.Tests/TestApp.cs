using Example.Job.Features.Customers;
using Example.Job.Integrations.Crm;
using Example.Job.Integrations.Erp;
using Example.Testing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Job.Tests;

/// <summary>
/// The job app's services, registered the way Program.cs registers them, with each integration's HTTP
/// calls going to a <see cref="FakeHttpHandler"/>. A new integration gets one property and its real
/// <c>Add&lt;Name&gt;Client</c> call here; a new job gets one registration. Then <see cref="Get{T}"/> the job
/// and call RunOnceAsync.
/// </summary>
public sealed class TestApp : IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public TestApp()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Crm:BaseAddress"] = "https://crm.test/",
                ["Erp:BaseAddress"] = "https://erp.test/",
            })
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(TimeProvider.System)
            .AddLogging();

        services.AddCrmClient().AddHttpClient<CrmClient>().ConfigurePrimaryHttpMessageHandler(() => Crm);
        services.AddErpClient().AddHttpClient<ErpClient>().ConfigurePrimaryHttpMessageHandler(() => Erp);

        services.AddOptions<SyncCustomers.Options>();
        services.AddSingleton<SyncCustomers.Job>();

        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    public FakeHttpHandler Crm { get; } = new();

    public FakeHttpHandler Erp { get; } = new();

    public T Get<T>()
        where T : notnull => _services.GetRequiredService<T>();

    public ValueTask DisposeAsync() => _services.DisposeAsync();
}
