using System.Net;
using System.Net.Http.Json;

namespace Example.Testing;

/// <summary>
/// Stub for an external HTTP API. Plug it into an integration's typed client in a TestApp, set up
/// outcomes with <see cref="Respond"/> or <see cref="Throw"/>, and assert on <see cref="Requests"/>.
/// Outcomes are used in order and the last one repeats.
/// </summary>
public sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Lock _lock = new();
    private readonly Queue<Func<HttpResponseMessage>> _outcomes = new();
    private readonly List<FakeHttpRequest> _requests = [];

    public IReadOnlyList<FakeHttpRequest> Requests
    {
        get
        {
            lock (_lock)
            {
                return [.. _requests];
            }
        }
    }

    /// <summary>Respond with <paramref name="status"/> and, if given, <paramref name="body"/> as JSON.</summary>
    public FakeHttpHandler Respond(HttpStatusCode status, object? body = null) => Enqueue(() => new HttpResponseMessage(status) { Content = body is null ? null : JsonContent.Create(body) });

    /// <summary>Throw instead of responding, e.g. <see cref="HttpRequestException"/> or <see cref="TaskCanceledException"/> for a timeout.</summary>
    public FakeHttpHandler Throw(Exception exception) => Enqueue(() => throw exception);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        Func<HttpResponseMessage> outcome;
        lock (_lock)
        {
            _requests.Add(new FakeHttpRequest(request.Method, request.RequestUri!, body));
            outcome = _outcomes.Count > 1 ? _outcomes.Dequeue()
                : _outcomes.TryPeek(out var last) ? last
                : throw new InvalidOperationException($"No outcome set up for {request.Method} {request.RequestUri}. Call Respond or Throw first.");
        }

        return outcome();
    }

    private FakeHttpHandler Enqueue(Func<HttpResponseMessage> outcome)
    {
        lock (_lock)
        {
            _outcomes.Enqueue(outcome);
        }

        return this;
    }
}

public sealed record FakeHttpRequest(HttpMethod Method, Uri Uri, string? Body);
