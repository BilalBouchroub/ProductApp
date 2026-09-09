using MediatR;
using ProductApp.Application.Products;
using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.Production.Queries;

public sealed record GetProductVersionsQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductVersionDto>>;
public sealed class GetProductVersionsQueryHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<GetProductVersionsQuery, IReadOnlyList<ProductVersionDto>>
{
    public async Task<IReadOnlyList<ProductVersionDto>> Handle(GetProductVersionsQuery request, CancellationToken cancellationToken)
    {
        _ = await repository.GetProductAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        return (await repository.ListVersionsAsync(request.ProductId, cancellationToken))
            .Select(version => version.ToDto()).ToArray();
    }
}

public sealed record GetProductionChainSummaryQuery(Guid ProductId, int VersionNumber)
    : IRequest<ProductionChainSummaryDto>;
public sealed class GetProductionChainSummaryQueryHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<GetProductionChainSummaryQuery, ProductionChainSummaryDto>
{
    public async Task<ProductionChainSummaryDto> Handle(GetProductionChainSummaryQuery request, CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(request.ProductId, request.VersionNumber, cancellationToken)
            ?? throw new ProductVersionNotFoundException(request.ProductId, request.VersionNumber);
        return ProductionChainCalculator.Calculate(version.ProductionSteps).ToDto();
    }
}

public sealed record GetProductionExperimentsQuery(Guid ProductId, int VersionNumber)
    : IRequest<IReadOnlyList<ProductionExperimentDto>>;
public sealed class GetProductionExperimentsQueryHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<GetProductionExperimentsQuery, IReadOnlyList<ProductionExperimentDto>>
{
    public async Task<IReadOnlyList<ProductionExperimentDto>> Handle(GetProductionExperimentsQuery request, CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(request.ProductId, request.VersionNumber, cancellationToken)
            ?? throw new ProductVersionNotFoundException(request.ProductId, request.VersionNumber);
        return (await repository.ListExperimentsAsync(version.Id, cancellationToken))
            .Select(experiment => experiment.ToDto()).ToArray();
    }
}

public sealed record GetProductionExperimentQuery(Guid ExperimentId) : IRequest<ProductionExperimentDto>;
public sealed class GetProductionExperimentQueryHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<GetProductionExperimentQuery, ProductionExperimentDto>
{
    public async Task<ProductionExperimentDto> Handle(GetProductionExperimentQuery request, CancellationToken cancellationToken)
    {
        var experiment = await repository.GetExperimentAsync(request.ExperimentId, cancellationToken)
            ?? throw new ProductionExperimentNotFoundException(request.ExperimentId);
        return experiment.ToDto();
    }
}
