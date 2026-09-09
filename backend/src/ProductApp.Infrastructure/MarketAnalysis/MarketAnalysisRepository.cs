using Microsoft.EntityFrameworkCore;
using ProductApp.Application.MarketAnalysis;
using ProductApp.Infrastructure.Persistence;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Infrastructure.MarketAnalysis;

public sealed class MarketAnalysisRepository(ApplicationDbContext dbContext)
    : IMarketAnalysisRepository
{
    public async Task<IReadOnlyList<MarketAnalysisEntity>> ListByProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.MarketAnalyses
            .AsNoTracking()
            .Where(analysis => analysis.ProductId == productId)
            .OrderByDescending(analysis => analysis.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        MarketAnalysisEntity analysis,
        CancellationToken cancellationToken)
    {
        await dbContext.MarketAnalyses.AddAsync(analysis, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
