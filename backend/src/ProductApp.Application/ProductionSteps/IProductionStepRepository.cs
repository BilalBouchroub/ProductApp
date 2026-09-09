using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.ProductionSteps;

public interface IProductionStepRepository
{
    Task<IReadOnlyList<ProductionStep>> ListAsync(Guid productId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionStep>> ListForUpdateAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductionStep?> GetByIdAsync(Guid productId, Guid stepId, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(
        Guid productId,
        int order,
        Guid? excludedStepId,
        CancellationToken cancellationToken);

    Task AddAsync(ProductionStep step, CancellationToken cancellationToken);

    void Remove(ProductionStep step);

    Task ReorderAsync(IReadOnlyList<ProductionStep> steps, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
