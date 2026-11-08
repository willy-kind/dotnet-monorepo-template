using Microsoft.Extensions.DependencyInjection;

namespace Example.Job.Tests;

/// <summary>
/// The job app's services, in memory. Register what Program.cs registers: the real
/// <c>Add&lt;Name&gt;Client</c> calls, each with its primary handler swapped for one
/// <see cref="Testing.FakeHttpHandler"/> property, and each job. Then <see cref="Get{T}"/> a job
/// and call RunOnceAsync.
/// </summary>
public sealed class TestApp : IAsyncDisposable
{
    private readonly ServiceProvider _services = new ServiceCollection()
        .AddLogging()
        .BuildServiceProvider();

    public T Get<T>()
        where T : notnull => _services.GetRequiredService<T>();

    public ValueTask DisposeAsync() => _services.DisposeAsync();
}
