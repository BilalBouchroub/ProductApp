using MediatR;
using ProductApp.Application.MarketAnalysis.DTOs;
using ProductApp.Application.Products;

namespace ProductApp.Application.MarketAnalysis.Queries.GetAnalysisHistory;

public sealed record GetAnalysisHistoryQuery(Guid ProductId)
    : IRequest<IReadOnlyList<MarketAnalysisDto>>;

public sealed class GetAnalysisHistoryQueryHandler(
    IMarketAnalysisRepository analysisRepository,
    IProductRepository productRepository)
    : IRequestHandler<GetAnalysisHistoryQuery, IReadOnlyList<MarketAnalysisDto>>
{
    public async Task<IReadOnlyList<MarketAnalysisDto>> Handle(
        GetAnalysisHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _ = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        var analyses = await analysisRepository.ListByProductAsync(
            request.ProductId, cancellationToken);
        return analyses.Select(analysis => analysis.ToDto()).ToArray();
    }
}
