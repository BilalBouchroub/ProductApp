using ProductApp.Application.MarketAnalysis.DTOs;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Application.MarketAnalysis;

internal static class MarketAnalysisMappings
{
    public static MarketAnalysisDto ToDto(this MarketAnalysisEntity analysis)
    {
        return new MarketAnalysisDto(
            analysis.Id,
            analysis.ProductId,
            analysis.MaterialCost,
            analysis.LaborCost,
            analysis.EquipmentCost,
            analysis.TotalProductionCost,
            analysis.TargetSellingPrice,
            analysis.GrossMargin,
            analysis.MarginRate,
            analysis.TotalCycleTimeMinutes,
            analysis.MaterialAvailabilityRate,
            analysis.FeasibilityScore,
            analysis.Recommendation,
            analysis.CreatedAtUtc);
    }
}
