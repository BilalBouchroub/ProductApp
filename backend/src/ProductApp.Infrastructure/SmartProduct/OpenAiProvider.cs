using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed class OpenAiProvider(
    HttpClient client,
    IOptions<SmartProductOptions> options,
    IConfiguration configuration) : IAiProvider
{
    private readonly SmartProductOptions settings = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public string Model => settings.Model;

    public async IAsyncEnumerable<AiProviderDelta> StreamAsync(AiProviderRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var key = configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(key)) throw new AiProviderUnavailableException("openai_not_configured");
        using var message = new HttpRequestMessage(HttpMethod.Post, "responses");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        message.Content = JsonContent.Create(new
        {
            model = settings.Model,
            instructions = request.Instructions,
            input = request.Messages.Select(item => new { role = item.Role, content = item.Content }),
            max_output_tokens = Math.Min(request.MaxOutputTokens, settings.MaximumOutputTokens),
            stream = true,
            store = false
        }, options: JsonOptions);
        using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) throw new AiProviderUnavailableException(MapStatus((int)response.StatusCode));

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ", StringComparison.Ordinal)) continue;
            var data = line[6..];
            if (data == "[DONE]") break;
            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeValue) ? typeValue.GetString() : null;
            if (type == "response.output_text.delta" && root.TryGetProperty("delta", out var delta))
                yield return new AiProviderDelta(delta.GetString(), false);
            else if (type == "response.completed" && root.TryGetProperty("response", out var completed))
            {
                int? input = null, output = null;
                if (completed.TryGetProperty("usage", out var usage))
                {
                    if (usage.TryGetProperty("input_tokens", out var inputValue)) input = inputValue.GetInt32();
                    if (usage.TryGetProperty("output_tokens", out var outputValue)) output = outputValue.GetInt32();
                }
                yield return new AiProviderDelta(null, true, input, output);
            }
            else if (type is "error" or "response.failed" or "response.incomplete")
                yield return new AiProviderDelta(null, true, ErrorCode: "openai_response_failed");
        }
    }

    public async Task<IReadOnlyList<float>?> CreateEmbeddingAsync(string text, CancellationToken ct)
    {
        var key = configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return null;
        using var message = new HttpRequestMessage(HttpMethod.Post, "embeddings");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        message.Content = JsonContent.Create(new { model = settings.EmbeddingModel, input = text }, options: JsonOptions);
        using var response = await client.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode) return null;
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return document.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray()
            .Select(value => value.GetSingle()).ToArray();
    }

    private static string MapStatus(int status) => status switch
    {
        401 or 403 => "openai_authentication_failed",
        429 => "openai_rate_limited",
        408 or 504 => "openai_timeout",
        _ when status >= 500 => "openai_unavailable",
        _ => "openai_request_rejected"
    };
}
