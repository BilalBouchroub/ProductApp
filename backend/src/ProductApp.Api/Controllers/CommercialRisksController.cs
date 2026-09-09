using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Commercial.Commands;
using ProductApp.Application.Commercial.DTOs;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Commercial)]
[Route("api/market-studies/{studyId:guid}/risks")]
public sealed class CommercialRisksController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RiskDto>> Create(Guid studyId, RiskValues request,
        CancellationToken cancellationToken)
    {
        var risk = await sender.Send(new CreateRiskCommand(studyId, request), cancellationToken);
        return Created($"/api/market-studies/{studyId}/risks/{risk.Id}", risk);
    }

    [HttpPut("{riskId:guid}")]
    public async Task<ActionResult<RiskDto>> Update(Guid studyId, Guid riskId,
        RiskValues request, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new UpdateRiskCommand(studyId, riskId, request), cancellationToken));
    }

    [HttpDelete("{riskId:guid}")]
    public async Task<IActionResult> Delete(Guid studyId, Guid riskId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteRiskCommand(studyId, riskId), cancellationToken);
        return NoContent();
    }
}
