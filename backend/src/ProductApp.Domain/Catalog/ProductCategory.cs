using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Domain.Catalog;

public sealed class ProductCategory : AuditableEntity
{
    private ProductCategory() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();
}
