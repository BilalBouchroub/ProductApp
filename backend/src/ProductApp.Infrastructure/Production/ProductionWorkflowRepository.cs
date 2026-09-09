using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Production;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Resources;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Production;

public sealed class ProductionWorkflowRepository(ApplicationDbContext dbContext)
    : IProductionWorkflowRepository
{
    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.Products.Include(product => product.Versions)
            .SingleOrDefaultAsync(product => product.Id == productId, cancellationToken);

    public Task<ProductVersion?> GetVersionAsync(Guid productId, int versionNumber, CancellationToken cancellationToken) =>
        dbContext.ProductVersions.AsSplitQuery()
            .Include(version => version.ProductionSteps).ThenInclude(step => step.Resources)
            .Include(version => version.Experiments).ThenInclude(experiment => experiment.Steps)
            .SingleOrDefaultAsync(version => version.ProductId == productId && version.VersionNumber == versionNumber, cancellationToken);

    public async Task<IReadOnlyList<ProductVersion>> ListVersionsAsync(Guid productId, CancellationToken cancellationToken) =>
        await dbContext.ProductVersions.AsNoTracking().Where(version => version.ProductId == productId)
            .OrderByDescending(version => version.VersionNumber).ToListAsync(cancellationToken);

    public Task<ProductionStep?> GetStepAsync(Guid stepId, CancellationToken cancellationToken) =>
        dbContext.ProductionSteps.Include(step => step.Resources)
            .SingleOrDefaultAsync(step => step.Id == stepId, cancellationToken);

    public Task<StepResource?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken) =>
        dbContext.StepResources.SingleOrDefaultAsync(resource => resource.Id == resourceId, cancellationToken);

    public Task<ProductionExperiment?> GetExperimentAsync(Guid experimentId, CancellationToken cancellationToken) =>
        dbContext.ProductionExperiments.Include(experiment => experiment.Steps)
            .SingleOrDefaultAsync(experiment => experiment.Id == experimentId, cancellationToken);

    public async Task<IReadOnlyList<ProductionExperiment>> ListExperimentsAsync(Guid productVersionId, CancellationToken cancellationToken) =>
        await dbContext.ProductionExperiments.AsNoTracking().Include(experiment => experiment.Steps)
            .Where(experiment => experiment.ProductVersionId == productVersionId)
            .OrderByDescending(experiment => experiment.StartDate).ToListAsync(cancellationToken);

    public void RemoveProduct(Product product) => dbContext.Products.Remove(product);
    public Task AddResourceAsync(StepResource resource, CancellationToken cancellationToken) =>
        dbContext.StepResources.AddAsync(resource, cancellationToken).AsTask();
    public void RemoveResource(StepResource resource) => dbContext.StepResources.Remove(resource);
    public Task AddExperimentAsync(ProductionExperiment experiment, CancellationToken cancellationToken) =>
        dbContext.ProductionExperiments.AddAsync(experiment, cancellationToken).AsTask();
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
