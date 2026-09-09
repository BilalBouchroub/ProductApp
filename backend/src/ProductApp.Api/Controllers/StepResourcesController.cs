using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Production.DTOs;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
[Route("api/production-steps/{stepId:guid}/resources")]
public sealed class StepResourcesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<StepResourceDto>> Add(Guid stepId,
        AddStepResourceDto request, CancellationToken cancellationToken)
    {
        var resource = await sender.Send(new AddStepResourceCommand(stepId, request.ResourceType,
            request.Designation, request.Unit, request.PlannedQuantity, request.ActualQuantity,
            request.UnitCost, request.AvailableStock, request.RawMaterialId, request.EquipmentId), cancellationToken);
        return Created($"/api/production-steps/{stepId}/resources/{resource.Id}", resource);
    }

    [HttpPut("{resourceId:guid}")]
    public async Task<ActionResult<StepResourceDto>> Update(Guid stepId, Guid resourceId,
        UpdateStepResourceDto request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new UpdateStepResourceCommand(stepId, resourceId, request.PlannedQuantity,
            request.ActualQuantity, request.UnitCost, request.AvailableStock), cancellationToken));

    [HttpDelete("{resourceId:guid}")]
    public async Task<IActionResult> Delete(Guid stepId, Guid resourceId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteStepResourceCommand(stepId, resourceId), cancellationToken);
        return NoContent();
    }
}
