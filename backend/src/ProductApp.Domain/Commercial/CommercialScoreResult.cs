namespace ProductApp.Domain.Commercial;

public sealed record CommercialScoreResult(decimal ProductionScore, decimal MarketScore,
    decimal FinancialScore, decimal RiskScore, decimal GlobalScore);
