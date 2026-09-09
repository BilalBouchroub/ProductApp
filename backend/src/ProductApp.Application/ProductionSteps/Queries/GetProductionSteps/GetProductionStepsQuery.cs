using MediatR;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps.DTOs;

namespace ProductApp.Application.ProductionSteps.Queries.GetProductionSteps;

public sealed record GetProductionStepsQuery(Guid ProductId)
    : IRequest<IReadOnlyList<ProductionStepDto>>;

public sealed class GetProductionStepsQueryHandler(
    IProductionStepRepository stepRepository,
    IProductRepository productRepository)
    : IRequestHandler<GetProductionStepsQuery, IReadOnlyList<ProductionStepDto>>
{
    public async Task<IReadOnlyList<ProductionStepDto>> Handle(
        GetProductionStepsQuery request,
        CancellationToken cancellationToken)
    {
        _ = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        var steps = await stepRepository.ListAsync(request.ProductId, cancellationToken);
        return steps.Select(step => step.ToDto()).ToArray();
    }
}
