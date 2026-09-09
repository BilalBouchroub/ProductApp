using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Administration;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/roles"), Authorize(Policy = SecurityPolicies.Administrator)]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RoleDto>> Get(CancellationToken ct) => sender.Send(new GetRolesQuery(), ct);
}
