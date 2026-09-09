using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Products;
using ProductApp.Domain.Products;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Products;

public sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = dbContext.Products.AsNoTracking()
            .AsSplitQuery()
            .Include(product => product.ProductCategory)
            .Include(product => product.Versions);
        if (!includeArchived)
        {
            query = query.Where(product => product.Status == ProductStatus.Active);
        }

        return await query.OrderBy(product => product.Code).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Products.AsSplitQuery().Include(product => product.ProductCategory)
            .Include(product => product.Versions)
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = Product.NormalizeCode(code);
        return dbContext.Products.AnyAsync(product => product.Code == normalizedCode, cancellationToken);
    }

    public Task<Guid?> FindCategoryIdAsync(string? categoryCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryCode)) return Task.FromResult<Guid?>(null);
        var normalized = categoryCode.Trim().Replace(' ', '_').ToUpperInvariant();
        return dbContext.ProductCategories.AsNoTracking()
            .Where(category => category.Code == normalized || category.Name.ToUpper() == categoryCode.Trim().ToUpper())
            .Select(category => (Guid?)category.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
