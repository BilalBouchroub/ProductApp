using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Administration;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/audit-logs"), Authorize(Policy = SecurityPolicies.Administrator)]
public sealed class AuditController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AuditLogDto>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? module = null, [FromQuery] AuditLevel? level = null,
        [FromQuery] Guid? userId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool descending = false, CancellationToken ct = default) =>
        sender.Send(new GetAuditLogsQuery(new(page, pageSize, search, sortBy, descending), module, level, userId, from, to), ct);
}
