using ProductApp.Domain.Common;
using ProductApp.Domain.MarketAnalysis;

namespace ProductApp.Application.Commercial.DTOs;

public sealed record MarketStudyValues(string Name, string TargetMarket, string GeographicArea,
    string CustomerSegment, DateTime StudyDate, decimal EstimatedMarketSize,
    decimal AnnualGrowthRate, decimal ProductionCost, decimal ProposedSalePrice,
    decimal AverageMarketPrice, decimal MonthlySalesVolume);
public sealed record CreateStudyDto(Guid ProductVersionId, MarketStudyValues Values);

public sealed record MarketStudyDto(Guid Id, Guid ProductVersionId, string Name,
    string TargetMarket, string GeographicArea, string CustomerSegment, DateTime StudyDate,
    MarketStudyStatus Status, decimal EstimatedMarketSize, decimal AnnualGrowthRate,
    decimal ProductionCost, decimal ProposedSalePrice, decimal AverageMarketPrice,
    decimal CalculatedMargin, decimal MarginRate, decimal MonthlySalesVolume,
    decimal AnnualRevenue, decimal ProductionScore, decimal MarketScore,
    decimal FinancialScore, decimal RiskScore, decimal GlobalScore,
    MarketRecommendation? Recommendation, IReadOnlyList<CompetitorDto> Competitors,
    IReadOnlyList<RiskDto> Risks, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record CompetitorDto(Guid Id, Guid MarketStudyId, string Name,
    string ProductName, decimal Price, decimal Quantity, decimal? EstimatedQualityScore,
    decimal? MarketShare, string? Strengths, string? Weaknesses,
    string? SalesChannels, decimal? CustomerRating);

public sealed record RiskDto(Guid Id, Guid MarketStudyId, string RiskType,
    int Probability, int Impact, int Severity, string Description, string? MitigationAction);

public sealed record CommercialResultDto(Guid StudyId, MarketRecommendation Recommendation,
    decimal ProductionScore, decimal MarketScore, decimal FinancialScore,
    decimal RiskScore, decimal GlobalScore, IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses, IReadOnlyList<string> MainRisks,
    IReadOnlyList<string> Improvements, decimal AdvisedPrice, int MinimumVolume);

public sealed record CompetitorValues(string Name, string ProductName, decimal Price,
    decimal Quantity, decimal? EstimatedQualityScore, decimal? MarketShare,
    string? Strengths, string? Weaknesses, string? SalesChannels, decimal? CustomerRating);

public sealed record RiskValues(string RiskType, int Probability, int Impact,
    string Description, string? MitigationAction);

public sealed record OptimizationRequestDto(Guid Id, Guid MarketStudyId, Guid ProductId,
    string ProductName, int ProductVersion, string Message, OptimizationPriority Priority,
    IReadOnlyList<string> RequestedChanges, OptimizationStatus Status, DateTime CreatedAt);
