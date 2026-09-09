using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.MarketAnalysis.Commands.RunMarketAnalysis;
using ProductApp.Application.MarketAnalysis.DTOs;
using ProductApp.Application.MarketAnalysis.Queries.GetAnalysisHistory;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Commercial)]
[Route("api/products/{productId:guid}/market-analysis")]
public sealed class MarketAnalysisController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MarketAnalysisDto>> Run(
        Guid productId,
        RunMarketAnalysisDto request,
        CancellationToken cancellationToken)
    {
        var materialRequirements = request.Materials
            .Select(material => new MaterialRequirement(material.SapCode, material.Quantity))
            .ToArray();
        var analysis = await sender.Send(
            new RunMarketAnalysisCommand(
                productId,
                request.TargetSellingPrice,
                request.EquipmentCost ?? 0m,
                materialRequirements),
            cancellationToken);
        return Ok(analysis);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<MarketAnalysisDto>>> GetHistory(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var analyses = await sender.Send(
            new GetAnalysisHistoryQuery(productId), cancellationToken);
        return Ok(analyses);
    }
}
