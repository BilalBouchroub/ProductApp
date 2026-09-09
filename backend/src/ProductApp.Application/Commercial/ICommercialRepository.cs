using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Commercial;

public interface ICommercialRepository
{
    Task<ProductVersion?> GetProductVersionAsync(Guid versionId, CancellationToken cancellationToken);
    Task<MarketStudy?> GetStudyAsync(Guid studyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MarketStudy>> ListStudiesAsync(Guid? productVersionId, CancellationToken cancellationToken);
    Task<Competitor?> GetCompetitorAsync(Guid competitorId, CancellationToken cancellationToken);
    Task<Risk?> GetRiskAsync(Guid riskId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<OptimizationRequest> Items, int TotalCount)> ListOptimizationRequestsAsync(
        int page, int pageSize, OptimizationStatus? status, CancellationToken cancellationToken);
    Task AddStudyAsync(MarketStudy study, CancellationToken cancellationToken);
    void RemoveStudy(MarketStudy study);
    Task AddCompetitorAsync(Competitor competitor, CancellationToken cancellationToken);
    void RemoveCompetitor(Competitor competitor);
    Task AddRiskAsync(Risk risk, CancellationToken cancellationToken);
    Task AddOptimizationRequestAsync(OptimizationRequest request, CancellationToken cancellationToken);
    void RemoveRisk(Risk risk);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
