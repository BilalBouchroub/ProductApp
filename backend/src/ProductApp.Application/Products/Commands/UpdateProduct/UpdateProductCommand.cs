using FluentValidation;
using MediatR;
using ProductApp.Application.Products.DTOs;

namespace ProductApp.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, string Name, string? Description,
    string? SapCode = null, string? ImageUrl = null, decimal? TargetSalePrice = null,
    decimal? BatchQuantity = null, string? ProductionUnit = null, string? CategoryCode = null,
    string? ThemeColor = null) : IRequest<ProductDto>;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(2000);
        RuleFor(command => command.SapCode).MaximumLength(50);
        RuleFor(command => command.ImageUrl).MaximumLength(2048);
        RuleFor(command => command.ThemeColor).Matches("^#[0-9A-Fa-f]{6}$").When(command => !string.IsNullOrWhiteSpace(command.ThemeColor));
        RuleFor(command => command.TargetSalePrice).GreaterThanOrEqualTo(0).When(command => command.TargetSalePrice.HasValue);
        RuleFor(command => command.BatchQuantity).GreaterThan(0).When(command => command.BatchQuantity.HasValue);
        RuleFor(command => command.ProductionUnit).MaximumLength(30);
    }
}

public sealed class UpdateProductCommandHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);
        var now = DateTime.UtcNow;
        product.Update(request.Name, request.Description, now);
        var categoryId = await repository.FindCategoryIdAsync(request.CategoryCode, cancellationToken);
        product.ConfigureTechnicalData(request.SapCode, request.ImageUrl, request.TargetSalePrice,
            request.BatchQuantity, request.ProductionUnit, categoryId, now);
        product.ConfigureTheme(request.ThemeColor, now);
        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }
}
