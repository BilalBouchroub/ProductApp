using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Production.DTOs;
using ProductApp.Application.Production.Queries;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/production"), Authorize(Policy = SecurityPolicies.Production)]
public sealed class ProductionController(ISender sender) : ControllerBase
{
    [HttpGet("products/{productId:guid}/versions/{versionNumber:int}/summary")]
    public Task<ProductionChainSummaryDto> Summary(Guid productId, int versionNumber, CancellationToken ct) => sender.Send(new GetProductionChainSummaryQuery(productId, versionNumber), ct);
    [HttpPost("products/{productId:guid}/versions/{versionNumber:int}/publish")]
    public Task<ProductVersionDto> Publish(Guid productId, int versionNumber, CancellationToken ct) => sender.Send(new PublishProductCommand(productId, versionNumber), ct);
}
