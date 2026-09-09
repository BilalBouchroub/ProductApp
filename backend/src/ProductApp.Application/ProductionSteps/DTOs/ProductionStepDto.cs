using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.Common;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.ProductionSteps.DTOs;

public sealed record ProductionStepDto(
    Guid Id,
    Guid ProductId,
    int Order,
    string Name,
    string Icon,
    string? Description,
    int DurationMinutes,
    int? ActualDurationMinutes,
    decimal? PlannedCost,
    decimal? ActualCost,
    decimal Temperature,
    decimal? Pressure,
    decimal? Humidity,
    decimal? PlannedOutputQuantity,
    decimal? ActualOutputQuantity,
    decimal? WasteQuantity,
    string EquipmentName,
    int OperatorCount,
    decimal LaborCost,
    decimal EnergyConsumption,
    string? Instructions,
    string? ValidationCriteria,
    string? Observations,
    RecordStatus Status,
    IReadOnlyList<StepResourceDto> Resources);

public sealed record CreateProductionStepDto(
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
    string Icon = ProductionStepIconNames.Automatic);

public sealed record UpdateProductionStepDto(
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
    string Icon = ProductionStepIconNames.Automatic);

public sealed record ReorderProductionStepsDto(IReadOnlyList<Guid> OrderedStepIds);
