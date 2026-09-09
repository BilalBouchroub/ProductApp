using ProductApp.Domain.Common;

namespace ProductApp.Application.Production.DTOs;

public sealed record CreateProductVersionDto(string? ChangeSummary);
public sealed record AddStepResourceDto(ResourceType ResourceType, string Designation,
    string Unit, decimal PlannedQuantity, decimal? ActualQuantity, decimal UnitCost,
    decimal? AvailableStock, Guid? RawMaterialId, Guid? EquipmentId);
public sealed record UpdateStepResourceDto(decimal PlannedQuantity, decimal? ActualQuantity,
    decimal UnitCost, decimal? AvailableStock);
public sealed record CreateProductionExperimentDto(string Name, string Objective,
    string? Hypothesis, DateTime StartDate, decimal PlannedQuantity,
    string? Observations = null, string? Conclusion = null);
public sealed record RecordExperimentStepActualsDto(decimal ActualCost,
    int ActualDurationMinutes, decimal? ActualOutputQuantity, string? Observations);
public sealed record CompleteProductionExperimentDto(decimal ActualQuantity,
    ExperimentResult Result, string? Observations, string? Conclusion);
public sealed record UpdateProductionExperimentNarrativeDto(string Objective,
    string? Hypothesis, string? Observations, string? Conclusion);
