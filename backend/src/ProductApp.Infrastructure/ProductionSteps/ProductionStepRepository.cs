using Microsoft.EntityFrameworkCore;
using ProductApp.Application.ProductionSteps;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.ProductionSteps;

public sealed class ProductionStepRepository(ApplicationDbContext dbContext)
    : IProductionStepRepository
{
    public async Task<IReadOnlyList<ProductionStep>> ListAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductionSteps
            .AsNoTracking()
            .Include(step => step.Resources)
            .Where(step => step.ProductId == productId
                && step.ProductVersionNumber == dbContext.Products
                    .Where(product => product.Id == productId)
                    .Select(product => product.VersionNumber)
                    .Single())
            .OrderBy(step => step.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionStep>> ListForUpdateAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProductionSteps
            .Where(step => step.ProductId == productId
                && step.ProductVersionNumber == dbContext.Products
                    .Where(product => product.Id == productId)
                    .Select(product => product.VersionNumber)
                    .Single())
            .OrderBy(step => step.Order)
            .ToListAsync(cancellationToken);
    }

    public Task<ProductionStep?> GetByIdAsync(
        Guid productId,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductionSteps.Include(step => step.Resources).SingleOrDefaultAsync(
            step => step.ProductId == productId && step.Id == stepId
                && step.ProductVersionNumber == dbContext.Products
                    .Where(product => product.Id == productId)
                    .Select(product => product.VersionNumber)
                    .Single(),
            cancellationToken);
    }

    public Task<bool> OrderExistsAsync(
        Guid productId,
        int order,
        Guid? excludedStepId,
        CancellationToken cancellationToken)
    {
        return dbContext.ProductionSteps.AnyAsync(
            step => step.ProductId == productId &&
                    step.ProductVersionNumber == dbContext.Products
                        .Where(product => product.Id == productId)
                        .Select(product => product.VersionNumber)
                        .Single() &&
                    step.Order == order &&
                    (!excludedStepId.HasValue || step.Id != excludedStepId.Value),
            cancellationToken);
    }

    public async Task AddAsync(ProductionStep step, CancellationToken cancellationToken)
    {
        await dbContext.ProductionSteps.AddAsync(step, cancellationToken);
    }

    public void Remove(ProductionStep step)
    {
        dbContext.ProductionSteps.Remove(step);
    }

    public async Task ReorderAsync(
        IReadOnlyList<ProductionStep> steps,
        CancellationToken cancellationToken)
    {
        if (steps.Count == 0)
        {
            return;
        }

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);
            var temporaryOrderStart = steps.Max(step => step.Order) + steps.Count;

            for (var index = 0; index < steps.Count; index++)
            {
                steps[index].ChangeOrder(temporaryOrderStart + index + 1);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            for (var index = 0; index < steps.Count; index++)
            {
                steps[index].ChangeOrder(index + 1);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
