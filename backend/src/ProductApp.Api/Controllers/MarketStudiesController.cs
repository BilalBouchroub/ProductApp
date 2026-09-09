using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Commercial.Commands;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Application.Commercial.Queries;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/market-studies"), Authorize(Policy = SecurityPolicies.Commercial)]
public sealed class MarketStudiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<MarketStudyDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] Guid? productVersionId = null, [FromQuery] MarketStudyStatus? status = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool descending = true, CancellationToken ct = default)
    {
        IEnumerable<MarketStudyDto> query = await sender.Send(new GetStudiesQuery(productVersionId), ct);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || x.TargetMarket.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) query = query.Where(x => x.Status == status);
        query = sortBy?.ToLowerInvariant() switch { "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name), "score" => descending ? query.OrderByDescending(x => x.GlobalScore) : query.OrderBy(x => x.GlobalScore), _ => descending ? query.OrderByDescending(x => x.StudyDate) : query.OrderBy(x => x.StudyDate) };
        var items = query.ToArray(); var size = Math.Clamp(pageSize, 1, 100); var current = Math.Max(page, 1);
        return new(items.Skip((current - 1) * size).Take(size).ToArray(), current, size, items.Length);
    }
    [HttpGet("{id:guid}")]
    public Task<MarketStudyDto> Get(Guid id, CancellationToken ct) => sender.Send(new GetStudyQuery(id), ct);
    [HttpPost]
    public async Task<ActionResult<MarketStudyDto>> Create(CreateStudyDto request, CancellationToken ct) { var item = await sender.Send(new CreateStudyCommand(request.ProductVersionId, request.Values), ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    [HttpPut("{id:guid}")]
    public Task<MarketStudyDto> Update(Guid id, MarketStudyValues request, CancellationToken ct) => sender.Send(new UpdateStudyCommand(id, request), ct);
    [HttpPut("{id:guid}/draft")]
    public Task<MarketStudyDto> SaveDraft(Guid id, MarketStudyValues request, CancellationToken ct) => sender.Send(new SaveDraftCommand(id, request), ct);
    [HttpPost("{id:guid}/validate")]
    public Task<CommercialResultDto> Validate(Guid id, CancellationToken ct) => sender.Send(new ValidateStudyCommand(id), ct);
    [HttpGet("{id:guid}/result")]
    public Task<CommercialResultDto> Result(Guid id, CancellationToken ct) => sender.Send(new GetStudyResultQuery(id), ct);
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await sender.Send(new DeleteStudyCommand(id), ct); return NoContent(); }
}
