using FluentValidation;
using MediatR;
using ProductApp.Application.Products;
using ProductApp.Domain.Products;

namespace ProductApp.Application.ProductionSteps.Commands.DeleteProductionStep;

public sealed record DeleteProductionStepCommand(Guid ProductId, Guid StepId) : IRequest;

public sealed class DeleteProductionStepCommandValidator : AbstractValidator<DeleteProductionStepCommand>
{
    public DeleteProductionStepCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.StepId).NotEmpty();
    }
}

public sealed class DeleteProductionStepCommandHandler(
    IProductionStepRepository stepRepository,
    IProductRepository productRepository)
    : IRequestHandler<DeleteProductionStepCommand>
{
    public async Task Handle(DeleteProductionStepCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        if (product.Status == ProductStatus.Archived)
        {
            throw new ProductArchivedException(product.Id);
        }

        var step = await stepRepository.GetByIdAsync(request.ProductId, request.StepId, cancellationToken)
            ?? throw new ProductionStepNotFoundException(request.StepId);
        stepRepository.Remove(step);
        await stepRepository.SaveChangesAsync(cancellationToken);
    }
}
