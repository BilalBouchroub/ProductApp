using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Administration;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;
using System.Security.Claims;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/users"), Authorize(Policy = SecurityPolicies.Administrator)]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<UserDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] UserStatus? status = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool descending = false, CancellationToken ct = default) =>
        sender.Send(new GetUsersQuery(new(page, pageSize, search, sortBy, descending), role, status), ct);

    [HttpGet("{id:guid}")]
    public Task<UserDto> Get(Guid id, CancellationToken ct) => sender.Send(new GetUserQuery(id), ct);

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var user = await sender.Send(new CreateUserCommand(request.FirstName, request.LastName, request.Email,
            request.PhoneNumber, request.Role, request.TemporaryPassword), ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public Task<UserDto> Update(Guid id, UpdateUserRequest request, CancellationToken ct) =>
        sender.Send(new UpdateUserCommand(id, request.FirstName, request.LastName, request.PhoneNumber), ct);

    [HttpPost("{id:guid}/actions")]
    public Task<UserActionResultDto> ExecuteAction(Guid id, UserActionRequest request, CancellationToken ct) =>
        sender.Send(new ExecuteUserActionCommand(id, CurrentUserId(), request.Action, request.Role), ct);

    private Guid CurrentUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException();
}

public sealed record CreateUserRequest(string FirstName, string LastName, string Email, string? PhoneNumber,
    string Role, string TemporaryPassword);
public sealed record UpdateUserRequest(string FirstName, string LastName, string? PhoneNumber);
public sealed record UserActionRequest(UserAdministrationAction Action, string? Role = null);
