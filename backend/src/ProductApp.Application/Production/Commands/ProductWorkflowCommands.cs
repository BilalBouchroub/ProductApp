using FluentValidation;
using MediatR;
using ProductApp.Application.Products;
using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Application.Common;
using ProductApp.Application.Auth;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Production.Commands;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest;
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{ public DeleteProductCommandValidator() => RuleFor(x => x.ProductId).NotEmpty(); }
public sealed class DeleteProductCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetProductAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        if (!product.CanBeDeleted()) throw new ProductionWorkflowException("Seul un produit dont la version courante est en brouillon peut être supprimé.");
        repository.RemoveProduct(product);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed record CreateProductVersionCommand(Guid ProductId, string? ChangeSummary)
    : IRequest<ProductVersionDto>;
public sealed class CreateProductVersionCommandValidator : AbstractValidator<CreateProductVersionCommand>
{
    public CreateProductVersionCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ChangeSummary).MaximumLength(2000);
    }
}
public sealed class CreateProductVersionCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<CreateProductVersionCommand, ProductVersionDto>
{
    public async Task<ProductVersionDto> Handle(CreateProductVersionCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetProductAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        var version = product.CreateNewVersion(request.ChangeSummary, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return version.ToDto();
    }
}

public sealed record PublishProductCommand(Guid ProductId, int VersionNumber)
    : IRequest<ProductVersionDto>;
public sealed class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VersionNumber).GreaterThan(0);
    }
}
public sealed class PublishProductCommandHandler(IProductionWorkflowRepository repository, INotificationService? notifications = null)
    : IRequestHandler<PublishProductCommand, ProductVersionDto>
{
    public async Task<ProductVersionDto> Handle(PublishProductCommand request, CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(request.ProductId, request.VersionNumber, cancellationToken)
            ?? throw new ProductVersionNotFoundException(request.ProductId, request.VersionNumber);
        var readiness = ProductionReadinessPolicy.Evaluate(version);
        if (!readiness.IsReady) throw new ProductionWorkflowException(string.Join(" ", readiness.Errors));
        version.Publish(DateTime.UtcNow);
        if (notifications is not null)
            await notifications.NotifyRoleAsync(AppRoles.CommercialManager, "Produit prêt pour étude",
                $"La version {version.VersionNumber} de {version.Name} est disponible pour une étude commerciale.",
                NotificationType.Information, nameof(ProductVersion), version.Id, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return version.ToDto();
    }
}
