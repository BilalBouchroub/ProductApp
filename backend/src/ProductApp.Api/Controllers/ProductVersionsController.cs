using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Production.DTOs;
using ProductApp.Application.Production.Queries;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
[Route("api/products/{productId:guid}/versions")]
public sealed class ProductVersionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductVersionDto>>> GetVersions(Guid productId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProductVersionsQuery(productId), cancellationToken));

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductVersionDto>> CreateVersion(Guid productId,
        CreateProductVersionDto request, CancellationToken cancellationToken)
    {
        var version = await sender.Send(new CreateProductVersionCommand(productId, request.ChangeSummary), cancellationToken);
        return CreatedAtAction(nameof(GetVersions), new { productId }, version);
    }

    [HttpGet("{versionNumber:int}/production-summary")]
    public async Task<ActionResult<ProductionChainSummaryDto>> GetSummary(Guid productId,
        int versionNumber, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProductionChainSummaryQuery(productId, versionNumber), cancellationToken));

    [HttpPost("{versionNumber:int}/publish")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductVersionDto>> Publish(Guid productId,
        int versionNumber, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new PublishProductCommand(productId, versionNumber), cancellationToken));

    [HttpGet("{versionNumber:int}/experiments")]
    public async Task<ActionResult<IReadOnlyList<ProductionExperimentDto>>> GetExperiments(Guid productId,
        int versionNumber, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProductionExperimentsQuery(productId, versionNumber), cancellationToken));

    [HttpPost("{versionNumber:int}/experiments")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductionExperimentDto>> CreateExperiment(Guid productId,
        int versionNumber, CreateProductionExperimentDto request, CancellationToken cancellationToken)
    {
        var experiment = await sender.Send(new CreateProductionExperimentCommand(productId,
            versionNumber, request.Name, request.Objective, request.Hypothesis,
            request.StartDate, request.PlannedQuantity, request.Observations,
            request.Conclusion), cancellationToken);
        return Created($"/api/production-experiments/{experiment.Id}", experiment);
    }
}
