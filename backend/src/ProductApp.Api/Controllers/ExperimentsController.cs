using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Common;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Production.DTOs;
using ProductApp.Application.Production.Queries;
using ProductApp.Domain.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/experiments"), Authorize]
public sealed class ExperimentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<ProductionExperimentDto>> GetAll(Guid productId, int versionNumber,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null,
        [FromQuery] ExperimentResult? result = null, [FromQuery] string? sortBy = null, [FromQuery] bool descending = true,
        CancellationToken ct = default)
    {
        IEnumerable<ProductionExperimentDto> query = await sender.Send(new GetProductionExperimentsQuery(productId, versionNumber), ct);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (result.HasValue) query = query.Where(x => x.Result == result);
        query = sortBy?.ToLowerInvariant() switch { "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name), "cost" => descending ? query.OrderByDescending(x => x.ActualCost) : query.OrderBy(x => x.ActualCost), _ => descending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate) };
        var items = query.ToArray(); var safeSize = Math.Clamp(pageSize, 1, 100); var safePage = Math.Max(page, 1);
        return new(items.Skip((safePage - 1) * safeSize).Take(safeSize).ToArray(), safePage, safeSize, items.Length);
    }
    [HttpGet("{id:guid}")]
    public Task<ProductionExperimentDto> Get(Guid id, CancellationToken ct) => sender.Send(new GetProductionExperimentQuery(id), ct);
    [HttpPost]
    [Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductionExperimentDto>> Create(CreateExperimentRequest request, CancellationToken ct)
    { var item = await sender.Send(new CreateProductionExperimentCommand(request.ProductId, request.VersionNumber, request.Name, request.Objective, request.Hypothesis, request.StartDate, request.PlannedQuantity, request.Observations, request.Conclusion), ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
}
public sealed record CreateExperimentRequest(Guid ProductId, int VersionNumber, string Name, string Objective, string? Hypothesis, DateTime StartDate, decimal PlannedQuantity, string? Observations = null, string? Conclusion = null);
