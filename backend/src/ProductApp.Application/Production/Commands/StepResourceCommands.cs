using FluentValidation;
using MediatR;
using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.Common;
using ProductApp.Domain.Resources;

namespace ProductApp.Application.Production.Commands;

public sealed record AddStepResourceCommand(Guid StepId, ResourceType ResourceType,
    string Designation, string Unit, decimal PlannedQuantity, decimal? ActualQuantity,
    decimal UnitCost, decimal? AvailableStock, Guid? RawMaterialId, Guid? EquipmentId)
    : IRequest<StepResourceDto>;
public sealed class AddStepResourceCommandValidator : AbstractValidator<AddStepResourceCommand>
{
    public AddStepResourceCommandValidator()
    {
        RuleFor(x => x.StepId).NotEmpty(); RuleFor(x => x.ResourceType).IsInEnum();
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PlannedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActualQuantity).GreaterThanOrEqualTo(0).When(x => x.ActualQuantity.HasValue);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AvailableStock).GreaterThanOrEqualTo(0).When(x => x.AvailableStock.HasValue);
    }
}
public sealed class AddStepResourceCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<AddStepResourceCommand, StepResourceDto>
{
    public async Task<StepResourceDto> Handle(AddStepResourceCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetStepAsync(request.StepId, cancellationToken)
            ?? throw new ProductionWorkflowException($"Étape '{request.StepId}' introuvable.");
        var resource = StepResource.Create(request.StepId, request.ResourceType,
            request.Designation, request.Unit, request.PlannedQuantity, request.ActualQuantity,
            request.UnitCost, request.AvailableStock, request.RawMaterialId, request.EquipmentId);
        await repository.AddResourceAsync(resource, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return resource.ToDto();
    }
}

public sealed record UpdateStepResourceCommand(Guid StepId, Guid ResourceId, decimal PlannedQuantity,
    decimal? ActualQuantity, decimal UnitCost, decimal? AvailableStock) : IRequest<StepResourceDto>;
public sealed class UpdateStepResourceCommandValidator : AbstractValidator<UpdateStepResourceCommand>
{
    public UpdateStepResourceCommandValidator()
    {
        RuleFor(x => x.StepId).NotEmpty(); RuleFor(x => x.ResourceId).NotEmpty(); RuleFor(x => x.PlannedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActualQuantity).GreaterThanOrEqualTo(0).When(x => x.ActualQuantity.HasValue);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AvailableStock).GreaterThanOrEqualTo(0).When(x => x.AvailableStock.HasValue);
    }
}
public sealed class UpdateStepResourceCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<UpdateStepResourceCommand, StepResourceDto>
{
    public async Task<StepResourceDto> Handle(UpdateStepResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await repository.GetResourceAsync(request.ResourceId, cancellationToken)
            ?? throw new StepResourceNotFoundException(request.ResourceId);
        if (resource.ProductionStepId != request.StepId) throw new StepResourceNotFoundException(request.ResourceId);
        resource.UpdateQuantities(request.PlannedQuantity, request.ActualQuantity,
            request.UnitCost, request.AvailableStock, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return resource.ToDto();
    }
}

public sealed record DeleteStepResourceCommand(Guid StepId, Guid ResourceId) : IRequest;
public sealed class DeleteStepResourceCommandValidator : AbstractValidator<DeleteStepResourceCommand>
{ public DeleteStepResourceCommandValidator() { RuleFor(x => x.StepId).NotEmpty(); RuleFor(x => x.ResourceId).NotEmpty(); } }
public sealed class DeleteStepResourceCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<DeleteStepResourceCommand>
{
    public async Task Handle(DeleteStepResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await repository.GetResourceAsync(request.ResourceId, cancellationToken)
            ?? throw new StepResourceNotFoundException(request.ResourceId);
        if (resource.ProductionStepId != request.StepId) throw new StepResourceNotFoundException(request.ResourceId);
        repository.RemoveResource(resource); await repository.SaveChangesAsync(cancellationToken);
    }
}
