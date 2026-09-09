using ProductApp.Application.Products.DTOs;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Products;

internal static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
            product.Code,
            product.Name,
            product.Description,
            product.VersionNumber,
            product.Status,
            product.CurrentVersion.Status,
            product.SapCode,
            product.ImageUrl,
            product.ThemeColor,
            product.TargetSalePrice,
            product.BatchQuantity,
            product.ProductionUnit,
            product.ProductCategoryId,
            product.ProductCategory?.Name,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }
}
