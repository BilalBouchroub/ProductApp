using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.ProductionSteps.Commands.CreateProductionStep;
using ProductApp.Application.ProductionSteps.Commands.DeleteProductionStep;
using ProductApp.Application.ProductionSteps.Commands.ReorderProductionSteps;
using ProductApp.Application.ProductionSteps.Commands.UpdateProductionStep;
using ProductApp.Application.ProductionSteps.DTOs;
using ProductApp.Application.ProductionSteps.Queries.GetProductionSteps;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
[Route("api/products/{productId:guid}/production-steps")]
public sealed class ProductionStepsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductionStepDto>>> GetAll(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var steps = await sender.Send(new GetProductionStepsQuery(productId), cancellationToken);
        return Ok(steps);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductionStepDto>> Create(
        Guid productId,
        CreateProductionStepDto request,
        CancellationToken cancellationToken)
    {
        var step = await sender.Send(
            new CreateProductionStepCommand(
                productId, request.Order, request.Name, request.Description,
                request.DurationMinutes, request.Temperature, request.EquipmentName, request.LaborCost,
                request.ActualDurationMinutes, request.PlannedCost, request.ActualCost,
                request.Pressure, request.Humidity, request.PlannedOutputQuantity,
                request.ActualOutputQuantity, request.WasteQuantity, request.OperatorCount,
                request.EnergyConsumption, request.Instructions, request.ValidationCriteria,
                request.Observations, request.Status, request.Icon),
            cancellationToken);
        return Created($"/api/products/{productId}/production-steps", step);
    }

    [HttpPut("{stepId:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductionStepDto>> Update(
        Guid productId,
        Guid stepId,
        UpdateProductionStepDto request,
        CancellationToken cancellationToken)
    {
        var step = await sender.Send(
            new UpdateProductionStepCommand(
                productId, stepId, request.Order, request.Name, request.Description,
                request.DurationMinutes, request.Temperature, request.EquipmentName, request.LaborCost,
                request.ActualDurationMinutes, request.PlannedCost, request.ActualCost,
                request.Pressure, request.Humidity, request.PlannedOutputQuantity,
                request.ActualOutputQuantity, request.WasteQuantity, request.OperatorCount,
                request.EnergyConsumption, request.Instructions, request.ValidationCriteria,
                request.Observations, request.Status, request.Icon),
            cancellationToken);
        return Ok(step);
    }

    [HttpDelete("{stepId:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<IActionResult> Delete(
        Guid productId,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductionStepCommand(productId, stepId), cancellationToken);
        return NoContent();
    }

    [HttpPut("reorder")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<IReadOnlyList<ProductionStepDto>>> Reorder(
        Guid productId,
        ReorderProductionStepsDto request,
        CancellationToken cancellationToken)
    {
        var steps = await sender.Send(
            new ReorderProductionStepsCommand(productId, request.OrderedStepIds),
            cancellationToken);
        return Ok(steps);
    }
}
