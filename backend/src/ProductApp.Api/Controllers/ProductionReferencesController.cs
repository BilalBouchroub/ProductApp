using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Production.References;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
[Route("api/production-references")]
public sealed class ProductionReferencesController(ISender sender) : ControllerBase
{
    [HttpGet("product-categories")]
    public async Task<ActionResult<IReadOnlyList<ProductCategoryOptionDto>>> GetProductCategories(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProductCategoriesQuery(), cancellationToken));

    [HttpGet("equipment")]
    public async Task<ActionResult<IReadOnlyList<EquipmentOptionDto>>> GetEquipment(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetEquipmentOptionsQuery(), cancellationToken));
}
