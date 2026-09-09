using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Administration;
using ProductApp.Application.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/notifications"), Authorize]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<NotificationDto>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? isRead = null, CancellationToken ct = default) =>
        sender.Send(new GetNotificationsQuery(UserId(), new(page, pageSize, search), isRead), ct);

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    { await sender.Send(new MarkNotificationReadCommand(UserId(), id), ct); return NoContent(); }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new ProductApp.Application.Auth.AuthenticationException("Authenticated user identifier is missing.");
}
