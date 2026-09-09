using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Commercial;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Commercial;

public sealed class CommercialRepository(ApplicationDbContext dbContext) : ICommercialRepository
{
    public Task<ProductVersion?> GetProductVersionAsync(Guid versionId, CancellationToken cancellationToken) =>
        dbContext.ProductVersions.AsSplitQuery()
            .Include(version => version.ProductionSteps).ThenInclude(step => step.Resources)
            .Include(version => version.Experiments).ThenInclude(experiment => experiment.Steps)
            .SingleOrDefaultAsync(version => version.Id == versionId, cancellationToken);

    public Task<MarketStudy?> GetStudyAsync(Guid studyId, CancellationToken cancellationToken) =>
        dbContext.MarketStudies.AsSplitQuery()
            .Include(study => study.Competitors).Include(study => study.Risks)
            .Include(study => study.ProductVersion).ThenInclude(version => version.Product)
            .Include(study => study.ProductVersion).ThenInclude(version => version.ProductionSteps).ThenInclude(step => step.Resources)
            .Include(study => study.ProductVersion).ThenInclude(version => version.Experiments).ThenInclude(experiment => experiment.Steps)
            .SingleOrDefaultAsync(study => study.Id == studyId, cancellationToken);

    public async Task<IReadOnlyList<MarketStudy>> ListStudiesAsync(Guid? productVersionId, CancellationToken cancellationToken)
    {
        var query = dbContext.MarketStudies.AsNoTracking().AsSplitQuery()
            .Include(study => study.Competitors).Include(study => study.Risks).AsQueryable();
        if (productVersionId.HasValue) query = query.Where(study => study.ProductVersionId == productVersionId.Value);
        return await query.OrderByDescending(study => study.StudyDate).ToListAsync(cancellationToken);
    }

    public Task<Competitor?> GetCompetitorAsync(Guid competitorId, CancellationToken cancellationToken) =>
        dbContext.Competitors.Include(competitor => competitor.MarketStudy)
            .SingleOrDefaultAsync(competitor => competitor.Id == competitorId, cancellationToken);

    public Task<Risk?> GetRiskAsync(Guid riskId, CancellationToken cancellationToken) =>
        dbContext.Risks.Include(risk => risk.MarketStudy)
            .SingleOrDefaultAsync(risk => risk.Id == riskId, cancellationToken);

    public async Task<(IReadOnlyList<OptimizationRequest> Items, int TotalCount)> ListOptimizationRequestsAsync(
        int page, int pageSize, OptimizationStatus? status, CancellationToken cancellationToken)
    {
        var query = dbContext.OptimizationRequests.AsNoTracking()
            .Include(request => request.MarketStudy).ThenInclude(study => study.ProductVersion)
            .ThenInclude(version => version.Product).AsQueryable();
        if (status.HasValue) query = query.Where(request => request.Status == status.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(request => request.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task AddStudyAsync(MarketStudy study, CancellationToken cancellationToken) =>
        dbContext.MarketStudies.AddAsync(study, cancellationToken).AsTask();
    public void RemoveStudy(MarketStudy study) => dbContext.MarketStudies.Remove(study);
    public Task AddCompetitorAsync(Competitor competitor, CancellationToken cancellationToken) =>
        dbContext.Competitors.AddAsync(competitor, cancellationToken).AsTask();
    public void RemoveCompetitor(Competitor competitor) => dbContext.Competitors.Remove(competitor);
    public Task AddRiskAsync(Risk risk, CancellationToken cancellationToken) =>
        dbContext.Risks.AddAsync(risk, cancellationToken).AsTask();
    public Task AddOptimizationRequestAsync(OptimizationRequest request, CancellationToken cancellationToken) =>
        dbContext.OptimizationRequests.AddAsync(request, cancellationToken).AsTask();
    public void RemoveRisk(Risk risk) => dbContext.Risks.Remove(risk);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
