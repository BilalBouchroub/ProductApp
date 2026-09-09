using FluentValidation;
using MediatR;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps.DTOs;
using ProductApp.Domain.Products;

namespace ProductApp.Application.ProductionSteps.Commands.ReorderProductionSteps;

public sealed record ReorderProductionStepsCommand(Guid ProductId, IReadOnlyList<Guid> OrderedStepIds)
    : IRequest<IReadOnlyList<ProductionStepDto>>;

public sealed class ReorderProductionStepsCommandValidator : AbstractValidator<ReorderProductionStepsCommand>
{
    public ReorderProductionStepsCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.OrderedStepIds).NotEmpty();
        RuleFor(command => command.OrderedStepIds)
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage("Production step identifiers must be unique.");
        RuleForEach(command => command.OrderedStepIds).NotEmpty();
    }
}

public sealed class ReorderProductionStepsCommandHandler(
    IProductionStepRepository stepRepository,
    IProductRepository productRepository)
    : IRequestHandler<ReorderProductionStepsCommand, IReadOnlyList<ProductionStepDto>>
{
    public async Task<IReadOnlyList<ProductionStepDto>> Handle(
        ReorderProductionStepsCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        if (product.Status == ProductStatus.Archived)
        {
            throw new ProductArchivedException(product.Id);
        }

        var steps = await stepRepository.ListForUpdateAsync(request.ProductId, cancellationToken);
        var stepsById = steps.ToDictionary(step => step.Id);
        if (steps.Count != request.OrderedStepIds.Count ||
            request.OrderedStepIds.Any(id => !stepsById.ContainsKey(id)))
        {
            throw new InvalidProductionStepOrderingException();
        }

        var orderedSteps = request.OrderedStepIds.Select(id => stepsById[id]).ToArray();
        await stepRepository.ReorderAsync(orderedSteps, cancellationToken);
        return orderedSteps.Select(step => step.ToDto()).ToArray();
    }
}
