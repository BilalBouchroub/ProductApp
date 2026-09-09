using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProductApp.Application.SmartProduct;
using ProductApp.Infrastructure.SmartProduct;

namespace ProductApp.Infrastructure.Tests.SmartProduct;

public sealed class GeminiProviderTests
{
    [Fact]
    public async Task StreamAsync_StreamsTextAndReturnsUsage()
    {
        const string sse = """
            data: {"candidates":[{"content":{"parts":[{"text":"Bonjour"}],"role":"model"}}]}

            data: {"candidates":[{"content":{"parts":[{"text":" ProductApp"}],"role":"model"},"finishReason":"STOP"}],"usageMetadata":{"promptTokenCount":12,"candidatesTokenCount":4,"thoughtsTokenCount":3,"totalTokenCount":19}}

            """;
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(sse, Encoding.UTF8, "text/event-stream")
        });
        var provider = CreateProvider(handler);

        var deltas = new List<AiProviderDelta>();
        await foreach (var delta in provider.StreamAsync(
                           new AiProviderRequest("Instruction", [new AiProviderMessage("user", "Bonjour")], 500),
                           CancellationToken.None))
            deltas.Add(delta);

        Assert.Equal("Bonjour ProductApp", string.Concat(deltas.Select(item => item.Text)));
        var completed = Assert.Single(deltas, item => item.IsCompleted);
        Assert.Equal(12, completed.InputTokens);
        Assert.Equal(7, completed.OutputTokens);
        Assert.Equal("models/gemini-3.6-flash:streamGenerateContent?alt=sse", handler.RequestUri);
        Assert.Equal("test-key", handler.ApiKey);
        Assert.Contains("systemInstruction", handler.RequestBody);
        Assert.Contains("role", handler.RequestBody);
        Assert.Contains("user", handler.RequestBody);
    }

    [Fact]
    public async Task StreamAsync_WhenKeyIsMissing_ReturnsSafeConfigurationCode()
    {
        var provider = CreateProvider(new StubHandler(_ => throw new InvalidOperationException()), null);

        var exception = await Assert.ThrowsAsync<AiProviderUnavailableException>(async () =>
        {
            await foreach (var _ in provider.StreamAsync(
                               new AiProviderRequest("Instruction", [new AiProviderMessage("user", "Bonjour")], 500),
                               CancellationToken.None))
            {
            }
        });

        Assert.Equal("gemini_not_configured", exception.SafeCode);
    }

    [Fact]
    public async Task StreamAsync_WhenQuotaIsExceeded_MapsRateLimitCode()
    {
        var provider = CreateProvider(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        var exception = await Assert.ThrowsAsync<AiProviderUnavailableException>(async () =>
        {
            await foreach (var _ in provider.StreamAsync(
                               new AiProviderRequest("Instruction", [new AiProviderMessage("user", "Bonjour")], 500),
                               CancellationToken.None))
            {
            }
        });

        Assert.Equal("gemini_rate_limited", exception.SafeCode);
    }

    [Fact]
    public async Task CreateEmbeddingAsync_ReturnsEmbeddingValues()
    {
        const string body = """
            {"embedding":{"values":[0.25,-0.5,0.75]},"usageMetadata":{"promptTokenCount":2,"totalTokenCount":2}}
            """;
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
        var provider = CreateProvider(handler);

        var values = await provider.CreateEmbeddingAsync("coût de production", CancellationToken.None);

        Assert.Equal([0.25f, -0.5f, 0.75f], values);
        Assert.Equal("models/gemini-embedding-001:embedContent", handler.RequestUri);
        Assert.Contains("outputDimensionality", handler.RequestBody);
        Assert.Contains("768", handler.RequestBody);
    }

    private static GeminiProvider CreateProvider(StubHandler handler, string? apiKey = "test-key")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GEMINI_API_KEY"] = apiKey
            })
            .Build();
        var options = Options.Create(new SmartProductOptions
        {
            Provider = "Gemini",
            Model = "gemini-3.6-flash",
            EmbeddingModel = "gemini-embedding-001",
            EmbeddingDimensions = 768
        });
        return new GeminiProvider(
            new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/") },
            options,
            configuration);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public string? RequestUri { get; private set; }
        public string? ApiKey { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri is null
                ? null
                : request.RequestUri.PathAndQuery.TrimStart('/').Replace("v1beta/", string.Empty);
            ApiKey = request.Headers.TryGetValues("x-goog-api-key", out var values)
                ? values.Single()
                : null;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responder(request);
        }
    }
}
