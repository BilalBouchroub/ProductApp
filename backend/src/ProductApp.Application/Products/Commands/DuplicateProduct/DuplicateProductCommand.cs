using FluentValidation;
using MediatR;
using ProductApp.Application.Products.DTOs;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Products.Commands.DuplicateProduct;

public sealed record DuplicateProductCommand(Guid SourceProductId, string Code) : IRequest<ProductDto>;

public sealed class DuplicateProductCommandValidator : AbstractValidator<DuplicateProductCommand>
{
    public DuplicateProductCommandValidator()
    {
        RuleFor(command => command.SourceProductId).NotEmpty();
        RuleFor(command => command.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9_-]+$");
    }
}

public sealed class DuplicateProductCommandHandler(IProductRepository repository)
    : IRequestHandler<DuplicateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(DuplicateProductCommand request, CancellationToken cancellationToken)
    {
        var source = await repository.GetByIdAsync(request.SourceProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.SourceProductId);
        var normalizedCode = Product.NormalizeCode(request.Code);
        if (await repository.CodeExistsAsync(normalizedCode, cancellationToken))
        {
            throw new ProductCodeConflictException(normalizedCode);
        }

        var duplicate = source.Duplicate(normalizedCode, DateTime.UtcNow);
        await repository.AddAsync(duplicate, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return duplicate.ToDto();
    }
}
