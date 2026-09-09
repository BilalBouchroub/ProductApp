using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Commercial;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/optimization-requests")]
public sealed class OptimizationRequestsController(ISender sender) : ControllerBase
{
    [HttpGet, Authorize(Policy = SecurityPolicies.Production)]
    public Task<PagedResult<OptimizationRequestDto>> Get(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] OptimizationStatus? status = null, CancellationToken ct = default) =>
        sender.Send(new GetOptimizationRequestsQuery(new PageRequest(page, pageSize), status), ct);

    [HttpPost, Authorize(Policy = SecurityPolicies.Commercial)]
    public async Task<ActionResult<OptimizationRequestDto>> Create(
        CreateOptimizationRequestRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateOptimizationRequestCommand(
            request.MarketStudyId, request.Message, request.Priority, request.RequestedChanges), ct);
        return CreatedAtAction(nameof(Get), new { page = 1, pageSize = 20 }, result);
    }
}

public sealed record CreateOptimizationRequestRequest(Guid MarketStudyId, string Message,
    OptimizationPriority Priority, IReadOnlyList<string> RequestedChanges);
