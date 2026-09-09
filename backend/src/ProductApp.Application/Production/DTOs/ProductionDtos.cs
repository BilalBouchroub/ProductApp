using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Production.DTOs;

public sealed record ProductVersionDto(Guid Id, Guid ProductId, int VersionNumber, string Name,
    string? Description, ProductVersionStatus Status, string? ChangeSummary, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record ProductionChainSummaryDto(int TotalDurationMinutes, decimal TotalCost,
    int ResourceCount, int MachineCount, decimal EnergyConsumption);

public sealed record StepResourceDto(Guid Id, Guid ProductionStepId, ResourceType ResourceType,
    string Designation, string Unit, decimal PlannedQuantity, decimal? ActualQuantity,
    decimal UnitCost, decimal TotalCost, decimal? AvailableStock, decimal? RemainingStock,
    AvailabilityStatus Availability);

public sealed record ExperimentStepDto(Guid Id, Guid ProductionStepId, int Order,
    decimal PlannedCost, decimal? ActualCost, int PlannedDurationMinutes,
    int? ActualDurationMinutes, decimal? PlannedOutputQuantity, decimal? ActualOutputQuantity,
    bool IsValidated);

public sealed record ProductionExperimentDto(Guid Id, Guid ProductVersionId, string Name,
    string Objective, string? Hypothesis,
    DateTime StartDate, DateTime? EndDate, decimal PlannedQuantity, decimal? ActualQuantity,
    decimal PlannedCost, decimal? ActualCost, int PlannedDurationMinutes,
    int? ActualDurationMinutes, decimal? WasteRate, ExperimentResult Result,
    string? Observations, string? Conclusion,
    IReadOnlyList<ExperimentStepDto> Steps, ExperimentSummaryDto? Summary);

public sealed record ExperimentSummaryDto(decimal ActualCost, int ActualDurationMinutes,
    decimal CostVariance, int DurationVarianceMinutes, decimal WasteRate,
    decimal PerformanceScore, decimal StabilityScore, string Summary);
