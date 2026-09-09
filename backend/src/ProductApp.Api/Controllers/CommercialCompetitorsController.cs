using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Commercial.Commands;
using ProductApp.Application.Commercial.DTOs;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Commercial)]
[Route("api/market-studies/{studyId:guid}/competitors")]
public sealed class CommercialCompetitorsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompetitorDto>> Create(Guid studyId, CompetitorValues request,
        CancellationToken cancellationToken)
    {
        var competitor = await sender.Send(new CreateCompetitorCommand(studyId, request), cancellationToken);
        return Created($"/api/market-studies/{studyId}/competitors/{competitor.Id}", competitor);
    }

    [HttpPut("{competitorId:guid}")]
    public async Task<ActionResult<CompetitorDto>> Update(Guid studyId, Guid competitorId,
        CompetitorValues request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateCompetitorCommand(studyId, competitorId, request), cancellationToken));
    }

    [HttpDelete("{competitorId:guid}")]
    public async Task<IActionResult> Delete(Guid studyId, Guid competitorId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCompetitorCommand(studyId, competitorId), cancellationToken);
        return NoContent();
    }
}
