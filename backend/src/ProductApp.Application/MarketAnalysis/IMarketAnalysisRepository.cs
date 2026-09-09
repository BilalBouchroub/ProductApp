using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Application.MarketAnalysis;

public interface IMarketAnalysisRepository
{
    Task<IReadOnlyList<MarketAnalysisEntity>> ListByProductAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task AddAsync(MarketAnalysisEntity analysis, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
