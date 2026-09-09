using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed class GeminiProvider(
    HttpClient client,
    IOptions<SmartProductOptions> options,
    IConfiguration configuration) : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SmartProductOptions settings = options.Value;

    public string Model => settings.Model;

    public async IAsyncEnumerable<AiProviderDelta> StreamAsync(
        AiProviderRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var key = configuration["GEMINI_API_KEY"];
        if (string.IsNullOrWhiteSpace(key))
            throw new AiProviderUnavailableException("gemini_not_configured");

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{Uri.EscapeDataString(settings.Model)}:streamGenerateContent?alt=sse");
        message.Headers.TryAddWithoutValidation("x-goog-api-key", key);
        message.Content = JsonContent.Create(new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = request.Instructions } }
            },
            contents = request.Messages.Select(item => new
            {
                role = item.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "model" : "user",
                parts = new[] { new { text = item.Content } }
            }),
            generationConfig = new
            {
                maxOutputTokens = Math.Min(request.MaxOutputTokens, settings.MaximumOutputTokens),
                temperature = request.Temperature
            }
        }, options: JsonOptions);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (TaskCanceledException exception) when (!ct.IsCancellationRequested)
        {
            throw new AiProviderUnavailableException("gemini_timeout", exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new AiProviderUnavailableException(MapStatus((int)response.StatusCode));

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            int? inputTokens = null;
            int? outputTokens = null;

            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;
                if (string.IsNullOrWhiteSpace(line) ||
                    !line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

                var data = line[5..].TrimStart();
                if (string.IsNullOrWhiteSpace(data) || data == "[DONE]") continue;

                var chunk = ParseChunk(data);
                inputTokens = chunk.InputTokens ?? inputTokens;
                outputTokens = chunk.OutputTokens ?? outputTokens;

                if (chunk.ErrorCode is not null)
                {
                    yield return new AiProviderDelta(null, true, inputTokens, outputTokens, chunk.ErrorCode);
                    yield break;
                }

                if (!string.IsNullOrEmpty(chunk.Text))
                    yield return new AiProviderDelta(chunk.Text, false);
            }

            yield return new AiProviderDelta(null, true, inputTokens, outputTokens);
        }
    }

    public async Task<IReadOnlyList<float>?> CreateEmbeddingAsync(string text, CancellationToken ct)
    {
        var key = configuration["GEMINI_API_KEY"];
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return null;

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"models/{Uri.EscapeDataString(settings.EmbeddingModel)}:embedContent");
        message.Headers.TryAddWithoutValidation("x-goog-api-key", key);
        message.Content = JsonContent.Create(new
        {
            model = $"models/{settings.EmbeddingModel}",
            content = new { parts = new[] { new { text } } },
            taskType = "SEMANTIC_SIMILARITY",
            outputDimensionality = Math.Clamp(settings.EmbeddingDimensions, 128, 3072)
        }, options: JsonOptions);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(message, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return null;
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode) return null;
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            if (!document.RootElement.TryGetProperty("embedding", out var embedding) ||
                !embedding.TryGetProperty("values", out var values)) return null;
            return values.EnumerateArray().Select(value => value.GetSingle()).ToArray();
        }
    }

    private static GeminiChunk ParseChunk(string data)
    {
        try
        {
            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;
            int? input = null;
            int? output = null;
            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                if (usage.TryGetProperty("promptTokenCount", out var inputValue) &&
                    inputValue.TryGetInt32(out var parsedInput))
                    input = parsedInput;
                var candidateTokens = 0;
                var thoughtTokens = 0;
                if (usage.TryGetProperty("candidatesTokenCount", out var outputValue) &&
                    outputValue.TryGetInt32(out var parsedOutput))
                    candidateTokens = parsedOutput;
                if (usage.TryGetProperty("thoughtsTokenCount", out var thoughtValue) &&
                    thoughtValue.TryGetInt32(out var parsedThoughts))
                    thoughtTokens = parsedThoughts;
                if (candidateTokens > 0 || thoughtTokens > 0)
                    output = candidateTokens + thoughtTokens;
            }

            if (root.TryGetProperty("promptFeedback", out var feedback) &&
                feedback.TryGetProperty("blockReason", out var blockReason) &&
                !string.IsNullOrWhiteSpace(blockReason.GetString()))
                return new GeminiChunk(null, input, output, "gemini_content_blocked");

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
                return new GeminiChunk(null, input, output, null);

            var candidate = candidates[0];
            if (candidate.TryGetProperty("finishReason", out var finishReason))
            {
                var reason = finishReason.GetString();
                if (reason is "SAFETY" or "RECITATION" or "PROHIBITED_CONTENT" or "BLOCKLIST")
                    return new GeminiChunk(null, input, output, "gemini_content_blocked");
                if (reason is "MALFORMED_FUNCTION_CALL" or "UNEXPECTED_TOOL_CALL" or "OTHER")
                    return new GeminiChunk(null, input, output, "gemini_response_failed");
            }

            if (!candidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts))
                return new GeminiChunk(null, input, output, null);

            var builder = new StringBuilder();
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("thought", out var thought) &&
                    thought.ValueKind == JsonValueKind.True) continue;
                if (part.TryGetProperty("text", out var textValue))
                    builder.Append(textValue.GetString());
            }
            return new GeminiChunk(builder.ToString(), input, output, null);
        }
        catch (JsonException exception)
        {
            throw new AiProviderUnavailableException("gemini_invalid_response", exception);
        }
    }

    private static string MapStatus(int status) => status switch
    {
        401 or 403 => "gemini_authentication_failed",
        404 => "gemini_model_unavailable",
        429 => "gemini_rate_limited",
        408 or 504 => "gemini_timeout",
        _ when status >= 500 => "gemini_unavailable",
        _ => "gemini_request_rejected"
    };

    private sealed record GeminiChunk(
        string? Text,
        int? InputTokens,
        int? OutputTokens,
        string? ErrorCode);
}
