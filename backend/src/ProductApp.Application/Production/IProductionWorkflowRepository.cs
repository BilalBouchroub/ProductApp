using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Resources;

namespace ProductApp.Application.Production;

public interface IProductionWorkflowRepository
{
    Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductVersion?> GetVersionAsync(Guid productId, int versionNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductVersion>> ListVersionsAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductionStep?> GetStepAsync(Guid stepId, CancellationToken cancellationToken);
    Task<StepResource?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken);
    Task<ProductionExperiment?> GetExperimentAsync(Guid experimentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductionExperiment>> ListExperimentsAsync(Guid productVersionId, CancellationToken cancellationToken);
    void RemoveProduct(Product product);
    Task AddResourceAsync(StepResource resource, CancellationToken cancellationToken);
    void RemoveResource(StepResource resource);
    Task AddExperimentAsync(ProductionExperiment experiment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
