using FluentValidation;
using MediatR;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps.DTOs;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Common;

namespace ProductApp.Application.ProductionSteps.Commands.CreateProductionStep;

public sealed record CreateProductionStepCommand(
    Guid ProductId,
    int Order,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Temperature,
    string EquipmentName,
    decimal LaborCost,
    int? ActualDurationMinutes = null,
    decimal? PlannedCost = null,
    decimal? ActualCost = null,
    decimal? Pressure = null,
    decimal? Humidity = null,
    decimal? PlannedOutputQuantity = null,
    decimal? ActualOutputQuantity = null,
    decimal? WasteQuantity = null,
    int OperatorCount = 0,
    decimal EnergyConsumption = 0,
    string? Instructions = null,
    string? ValidationCriteria = null,
    string? Observations = null,
    RecordStatus Status = RecordStatus.Active,
    string Icon = ProductionStepIconNames.Automatic) : IRequest<ProductionStepDto>;

public sealed class CreateProductionStepCommandValidator : AbstractValidator<CreateProductionStepCommand>
{
    public CreateProductionStepCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Order).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Icon).NotEmpty().MaximumLength(50)
            .Must(icon => ProductionStepIcons.All.Contains(icon));
        RuleFor(command => command.Description).MaximumLength(2000);
        RuleFor(command => command.DurationMinutes).InclusiveBetween(1, 43200);
        RuleFor(command => command.Temperature).InclusiveBetween(-273.15m, 2000m);
        RuleFor(command => command.EquipmentName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.LaborCost).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(command => command.OperatorCount).GreaterThanOrEqualTo(0);
        RuleFor(command => command.EnergyConsumption).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Humidity).InclusiveBetween(0, 100).When(command => command.Humidity.HasValue);
    }
}

public sealed class CreateProductionStepCommandHandler(
    IProductionStepRepository stepRepository,
    IProductRepository productRepository)
    : IRequestHandler<CreateProductionStepCommand, ProductionStepDto>
{
    public async Task<ProductionStepDto> Handle(
        CreateProductionStepCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        if (product.Status == ProductStatus.Archived)
        {
            throw new ProductArchivedException(product.Id);
        }

        if (await stepRepository.OrderExistsAsync(
            request.ProductId, request.Order, null, cancellationToken))
        {
            throw new ProductionStepOrderConflictException(request.ProductId, request.Order);
        }

        var step = ProductionStep.CreateForVersion(
            request.ProductId, product.VersionNumber, request.Order, request.Name, request.Description,
            request.DurationMinutes, request.Temperature, request.EquipmentName, request.LaborCost);
        step.SetIcon(request.Icon);
        step.ConfigureDetails(request.ActualDurationMinutes, request.PlannedCost, request.ActualCost,
            request.Pressure, request.Humidity, request.PlannedOutputQuantity,
            request.ActualOutputQuantity, request.WasteQuantity, request.OperatorCount,
            request.EnergyConsumption, request.Instructions, request.ValidationCriteria,
            request.Observations, request.Status);
        await stepRepository.AddAsync(step, cancellationToken);
        await stepRepository.SaveChangesAsync(cancellationToken);
        return step.ToDto();
    }
}
