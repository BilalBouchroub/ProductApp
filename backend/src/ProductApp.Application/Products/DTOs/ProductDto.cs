using ProductApp.Domain.Products;

namespace ProductApp.Application.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int VersionNumber,
    ProductStatus Status,
    ProductVersionStatus VersionStatus,
    string? SapCode,
    string? ImageUrl,
    string ThemeColor,
    decimal? TargetSalePrice,
    decimal? BatchQuantity,
    string? ProductionUnit,
    Guid? ProductCategoryId,
    string? CategoryName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateProductDto(string Code, string Name, string? Description,
    string? SapCode = null, string? ImageUrl = null, decimal? TargetSalePrice = null,
    decimal? BatchQuantity = null, string? ProductionUnit = null, string? CategoryCode = null,
    string? ThemeColor = null);

public sealed record UpdateProductDto(string Name, string? Description,
    string? SapCode = null, string? ImageUrl = null, decimal? TargetSalePrice = null,
    decimal? BatchQuantity = null, string? ProductionUnit = null, string? CategoryCode = null,
    string? ThemeColor = null);

public sealed record DuplicateProductDto(string Code);
