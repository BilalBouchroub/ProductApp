using ProductApp.Domain.Products;

namespace ProductApp.Application.Products;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    Task<Guid?> FindCategoryIdAsync(string? categoryCode, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
