using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.SapIntegration;
using ProductApp.Application.SapIntegration.DTOs;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
[Route("api/sap")]
public sealed class SapIntegrationController(ISapDataProvider sapDataProvider) : ControllerBase
{
    [HttpGet("materials")]
    public async Task<ActionResult<IReadOnlyList<SapMaterialDto>>> GetMaterials(
        CancellationToken cancellationToken)
    {
        return Ok(await sapDataProvider.GetMaterialsAsync(cancellationToken));
    }

    [HttpGet("boms")]
    public async Task<ActionResult<IReadOnlyList<SapBomDto>>> GetBoms(
        CancellationToken cancellationToken)
    {
        return Ok(await sapDataProvider.GetBomsAsync(cancellationToken));
    }

    [HttpGet("production-orders")]
    public async Task<ActionResult<IReadOnlyList<SapProductionOrderDto>>> GetProductionOrders(
        CancellationToken cancellationToken)
    {
        return Ok(await sapDataProvider.GetProductionOrdersAsync(cancellationToken));
    }
}
