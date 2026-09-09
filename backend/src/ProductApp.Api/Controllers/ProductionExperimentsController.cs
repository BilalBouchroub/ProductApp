using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Production.DTOs;
using ProductApp.Application.Production.Queries;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
[Route("api/production-experiments/{experimentId:guid}")]
public sealed class ProductionExperimentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductionExperimentDto>> Get(Guid experimentId,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProductionExperimentQuery(experimentId), cancellationToken));

    [HttpPut("steps/{experimentStepId:guid}/actuals")]
    public async Task<ActionResult<ProductionExperimentDto>> RecordActuals(Guid experimentId,
        Guid experimentStepId, RecordExperimentStepActualsDto request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new RecordExperimentStepActualsCommand(experimentId,
            experimentStepId, request.ActualCost, request.ActualDurationMinutes,
            request.ActualOutputQuantity, request.Observations), cancellationToken));

    [HttpPut("narrative")]
    public async Task<ActionResult<ProductionExperimentDto>> UpdateNarrative(Guid experimentId,
        UpdateProductionExperimentNarrativeDto request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new UpdateProductionExperimentNarrativeCommand(experimentId,
            request.Objective, request.Hypothesis, request.Observations, request.Conclusion),
            cancellationToken));

    [HttpPost("complete")]
    public async Task<ActionResult<ExperimentSummaryDto>> Complete(Guid experimentId,
        CompleteProductionExperimentDto request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new CompleteProductionExperimentCommand(experimentId,
            request.ActualQuantity, request.Result, request.Observations, request.Conclusion), cancellationToken));
}
