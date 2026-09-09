using FluentValidation;
using MediatR;
using ProductApp.Application.Products.DTOs;

namespace ProductApp.Application.Products.Commands.ArchiveProduct;

public sealed record ArchiveProductCommand(Guid Id) : IRequest<ProductDto>;

public sealed class ArchiveProductCommandValidator : AbstractValidator<ArchiveProductCommand>
{
    public ArchiveProductCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class ArchiveProductCommandHandler(IProductRepository repository)
    : IRequestHandler<ArchiveProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);
        product.Archive(DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }
}
