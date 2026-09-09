using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;

namespace ProductApp.Application.Commercial;

internal static class CommercialMappings
{
    public static MarketStudyDto ToDto(this MarketStudy study) => new(study.Id,
        study.ProductVersionId, study.Name, study.TargetMarket, study.GeographicArea,
        study.CustomerSegment, study.StudyDate, study.Status, study.EstimatedMarketSize,
        study.AnnualGrowthRate, study.ProductionCost, study.ProposedSalePrice,
        study.AverageMarketPrice, study.CalculatedMargin, study.MarginRate,
        study.MonthlySalesVolume, study.AnnualRevenue, study.ProductionScore,
        study.MarketScore, study.FinancialScore, study.RiskScore, study.GlobalScore,
        study.Recommendation, study.Competitors.Select(ToDto).ToArray(),
        study.Risks.Select(ToDto).ToArray(), study.CreatedAt, study.UpdatedAt);

    public static CompetitorDto ToDto(this Competitor competitor) => new(competitor.Id,
        competitor.MarketStudyId, competitor.Name, competitor.ProductName, competitor.Price,
        competitor.Quantity, competitor.EstimatedQualityScore, competitor.MarketShare,
        competitor.Strengths, competitor.Weaknesses, competitor.SalesChannels,
        competitor.CustomerRating);

    public static RiskDto ToDto(this Risk risk) => new(risk.Id, risk.MarketStudyId,
        risk.RiskType, risk.Probability, risk.Impact, risk.Severity,
        risk.Description, risk.MitigationAction);

    public static OptimizationRequestDto ToDto(this OptimizationRequest request) =>
        new(request.Id, request.MarketStudyId, request.MarketStudy.ProductVersion.ProductId,
            request.MarketStudy.ProductVersion.Product.Name,
            request.MarketStudy.ProductVersion.VersionNumber, request.Message, request.Priority,
            request.RequestedChanges, request.Status, request.CreatedAt);
}
