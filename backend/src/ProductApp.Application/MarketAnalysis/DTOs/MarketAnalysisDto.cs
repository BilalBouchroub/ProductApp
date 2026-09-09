using ProductApp.Domain.MarketAnalysis;

namespace ProductApp.Application.MarketAnalysis.DTOs;

public sealed record MaterialRequirementDto(string SapCode, decimal Quantity);

public sealed record RunMarketAnalysisDto(
    decimal TargetSellingPrice,
    decimal? EquipmentCost,
    IReadOnlyList<MaterialRequirementDto> Materials);

public sealed record MarketAnalysisDto(
    Guid Id,
    Guid ProductId,
    decimal MaterialCost,
    decimal LaborCost,
    decimal EquipmentCost,
    decimal TotalProductionCost,
    decimal TargetSellingPrice,
    decimal GrossMargin,
    decimal MarginRate,
    int TotalCycleTimeMinutes,
    decimal MaterialAvailabilityRate,
    decimal FeasibilityScore,
    MarketRecommendation Recommendation,
    DateTime CreatedAtUtc);
