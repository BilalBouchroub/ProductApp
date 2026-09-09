using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductApp.Application.Auth;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/ai/conversations"), Authorize, EnableRateLimiting("smart-product")]
public sealed class AiConversationsController(ISmartProductService service) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> ActiveGenerations = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost]
    public async Task<ActionResult<AiConversationDto>> Create([FromBody] CreateAiConversationRequest request, CancellationToken ct) =>
        Created(string.Empty, await service.CreateConversationAsync(CurrentUser(), request, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiConversationListItemDto>>> Search(
        [FromQuery] string? query, [FromQuery] bool includeArchived, CancellationToken ct) =>
        Ok(await service.SearchConversationsAsync(CurrentUser(), query, includeArchived, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AiConversationDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await service.GetConversationAsync(CurrentUser(), id, ct));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AiConversationDto>> Update(Guid id, [FromBody] UpdateAiConversationRequest request, CancellationToken ct) =>
        Ok(await service.UpdateConversationAsync(CurrentUser(), id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteConversationAsync(CurrentUser(), id, ct); return NoContent();
    }

    [HttpPost("{id:guid}/messages")]
    public async Task SendMessage(Guid id, [FromBody] SendAiMessageRequest request, CancellationToken ct)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-transform";
        Response.Headers.Append("X-Accel-Buffering", "no");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (ActiveGenerations.TryRemove(id, out var previous)) { previous.Cancel(); previous.Dispose(); }
        ActiveGenerations[id] = linked;
        try
        {
            await foreach (var item in service.SendMessageAsync(CurrentUser(), id, request, linked.Token))
                await WriteEventAsync(item, linked.Token);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested) { }
        catch (AiProviderUnavailableException exception)
        {
            await WriteEventAsync(new AiStreamEvent("response.error", new
            { code = exception.SafeCode, message = "SMART PRODUCT est temporairement indisponible." }), CancellationToken.None);
        }
        finally
        {
            ActiveGenerations.TryRemove(new KeyValuePair<Guid, CancellationTokenSource>(id, linked));
        }
    }

    [HttpPost("{id:guid}/stop")]
    public IActionResult Stop(Guid id)
    {
        if (ActiveGenerations.TryGetValue(id, out var generation)) generation.Cancel();
        return Accepted(new { stopped = generation is not null });
    }

    [HttpPost("{id:guid}/attachments"), RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<AiAttachmentDto>> Upload(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest();
        await using var stream = file.OpenReadStream();
        return Ok(await service.UploadAsync(CurrentUser(), id, stream, file.FileName,
            file.ContentType, file.Length, ct));
    }

    [HttpPost("~/api/ai/messages/{messageId:guid}/regenerate")]
    public async Task Regenerate(Guid messageId, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream"; Response.Headers.CacheControl = "no-cache, no-transform";
        await foreach (var item in service.RegenerateAsync(CurrentUser(), messageId, ct)) await WriteEventAsync(item, ct);
    }

    [HttpPost("~/api/ai/messages/{messageId:guid}/feedback")]
    public async Task<IActionResult> Feedback(Guid messageId, [FromBody] AiFeedbackRequest request, CancellationToken ct)
    {
        await service.SaveFeedbackAsync(CurrentUser(), messageId, request, ct); return NoContent();
    }

    private AiUserContext CurrentUser()
    {
        var id = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var value) ? value : throw new AuthenticationException("Identifiant utilisateur absent.");
        return new AiUserContext(id, User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Utilisateur ProductApp");
    }

    private async Task WriteEventAsync(AiStreamEvent item, CancellationToken ct)
    {
        await Response.WriteAsync($"event: {item.Type}\n", ct);
        await Response.WriteAsync($"data: {JsonSerializer.Serialize(item.Data, JsonOptions)}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
