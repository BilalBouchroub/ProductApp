using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Products.Commands.ArchiveProduct;
using ProductApp.Application.Products.Commands.CreateProduct;
using ProductApp.Application.Products.Commands.DuplicateProduct;
using ProductApp.Application.Products.Commands.UpdateProduct;
using ProductApp.Application.Products.DTOs;
using ProductApp.Application.Products.Images;
using ProductApp.Application.Products.Queries.GetProductById;
using ProductApp.Application.Products.Queries.GetProducts;
using ProductApp.Application.Production.Commands;
using ProductApp.Application.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] bool includeArchived, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] ProductStatus? status = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool descending = false,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ProductDto> products = await sender.Send(new GetProductsQuery(includeArchived), cancellationToken);
        if (!string.IsNullOrWhiteSpace(search)) products = products.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) products = products.Where(x => x.Status == status);
        products = sortBy?.ToLowerInvariant() switch
        { "code" => descending ? products.OrderByDescending(x => x.Code) : products.OrderBy(x => x.Code), "createdat" => descending ? products.OrderByDescending(x => x.CreatedAtUtc) : products.OrderBy(x => x.CreatedAtUtc), "updatedat" => descending ? products.OrderByDescending(x => x.UpdatedAtUtc) : products.OrderBy(x => x.UpdatedAtUtc), _ => descending ? products.OrderByDescending(x => x.Name) : products.OrderBy(x => x.Name) };
        var all = products.ToArray(); var size = Math.Clamp(pageSize, 1, 100); var current = Math.Max(page, 1);
        return Ok(new PagedResult<ProductDto>(all.Skip((current - 1) * size).Take(size).ToArray(), current, size, all.Length));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(product);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductDto request,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(
            new CreateProductCommand(request.Code, request.Name, request.Description, request.SapCode,
                request.ImageUrl, request.TargetSalePrice, request.BatchQuantity,
                request.ProductionUnit, request.CategoryCode, request.ThemeColor),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductImageDto>> UploadImage(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var relativeUrl = await sender.Send(
            new UploadProductImageCommand(stream, file.FileName, file.ContentType, file.Length),
            cancellationToken);
        var absoluteUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{relativeUrl}";
        return Ok(new ProductImageDto(absoluteUrl));
    }

    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        UpdateProductDto request,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(
            new UpdateProductCommand(id, request.Name, request.Description, request.SapCode,
                request.ImageUrl, request.TargetSalePrice, request.BatchQuantity,
                request.ProductionUnit, request.CategoryCode, request.ThemeColor),
            cancellationToken);
        return Ok(product);
    }

    [HttpPost("{id:guid}/archive")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new ArchiveProductCommand(id), cancellationToken);
        return Ok(product);
    }

    [HttpPost("{id:guid}/duplicate")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<ActionResult<ProductDto>> Duplicate(
        Guid id,
        DuplicateProductDto request,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(
            new DuplicateProductCommand(id, request.Code),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpDelete("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = SecurityPolicies.Production)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record ProductImageDto(string Url);
