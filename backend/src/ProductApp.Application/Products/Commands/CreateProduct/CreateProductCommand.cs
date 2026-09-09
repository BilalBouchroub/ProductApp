using FluentValidation;
using MediatR;
using ProductApp.Application.Products.DTOs;
using ProductApp.Domain.Products;
using ProductApp.Application.Common;
using ProductApp.Application.Auth;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Code, string Name, string? Description,
    string? SapCode = null, string? ImageUrl = null, decimal? TargetSalePrice = null,
    decimal? BatchQuantity = null, string? ProductionUnit = null, string? CategoryCode = null,
    string? ThemeColor = null)
    : IRequest<ProductDto>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9_-]+$");
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

public sealed class CreateProductCommandHandler(IProductRepository repository, INotificationService? notifications = null)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = Product.NormalizeCode(request.Code);
        if (await repository.CodeExistsAsync(normalizedCode, cancellationToken))
        {
            throw new ProductCodeConflictException(normalizedCode);
        }

        var now = DateTime.UtcNow;
        var product = Product.Create(normalizedCode, request.Name, request.Description, now);
        var categoryId = await repository.FindCategoryIdAsync(request.CategoryCode, cancellationToken);
        product.ConfigureTechnicalData(request.SapCode, request.ImageUrl, request.TargetSalePrice,
            request.BatchQuantity, request.ProductionUnit, categoryId, now);
        product.ConfigureTheme(request.ThemeColor, now);
        await repository.AddAsync(product, cancellationToken);
        if (notifications is not null)
            await notifications.NotifyRoleAsync(AppRoles.ProductionManager, "Produit créé",
                $"Le produit {product.Name} a été créé.", NotificationType.Success,
                nameof(Product), product.Id, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }
}
