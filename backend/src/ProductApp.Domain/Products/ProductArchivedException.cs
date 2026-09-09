namespace ProductApp.Domain.Products;

public sealed class ProductArchivedException(Guid productId)
    : InvalidOperationException($"Product '{productId}' is archived.")
{
    public Guid ProductId { get; } = productId;
}
