namespace ProductApp.Application.Products;

public sealed class ProductNotFoundException(Guid productId)
    : Exception($"Product '{productId}' was not found.")
{
    public Guid ProductId { get; } = productId;
}

public sealed class ProductCodeConflictException(string code)
    : Exception($"A product with code '{code}' already exists.")
{
    public string Code { get; } = code;
}
