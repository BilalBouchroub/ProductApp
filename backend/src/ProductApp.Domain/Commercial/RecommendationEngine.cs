using ProductApp.Domain.MarketAnalysis;

namespace ProductApp.Domain.Commercial;

public sealed record CommercialRecommendationResult(MarketRecommendation Recommendation,
    IReadOnlyList<string> Strengths, IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> MainRisks, IReadOnlyList<string> Improvements,
    decimal AdvisedPrice, int MinimumVolume);

public sealed class RecommendationEngine
{
    public CommercialRecommendationResult Generate(MarketStudy study, CommercialScoreResult scores)
    {
        ArgumentNullException.ThrowIfNull(study);
        var recommendation = scores.GlobalScore >= 70m && scores.FinancialScore >= 60m && scores.RiskScore >= 50m
            ? MarketRecommendation.Viable
            : scores.GlobalScore >= 45m && scores.FinancialScore >= 35m && scores.RiskScore >= 25m
                ? MarketRecommendation.ToOptimize : MarketRecommendation.NotViable;

        var strengths = new List<string>(); var weaknesses = new List<string>(); var improvements = new List<string>();
        AddCriterion(scores.ProductionScore, "Processus de production maîtrisé", "Processus de production insuffisamment maîtrisé", "Stabiliser la chaîne et réduire les écarts de production", strengths, weaknesses, improvements);
        AddCriterion(scores.MarketScore, "Potentiel de marché favorable", "Potentiel de marché limité", "Affiner le ciblage et renforcer la différenciation", strengths, weaknesses, improvements);
        AddCriterion(scores.FinancialScore, "Rentabilité prévisionnelle solide", "Rentabilité prévisionnelle fragile", "Réduire les coûts ou repositionner le prix", strengths, weaknesses, improvements);
        AddCriterion(scores.RiskScore, "Exposition aux risques maîtrisée", "Exposition aux risques élevée", "Mettre en œuvre les plans de mitigation prioritaires", strengths, weaknesses, improvements);

        var mainRisks = study.Risks.OrderByDescending(risk => risk.Severity).Take(5)
            .Select(risk => $"{risk.RiskType} ({risk.Probability}×{risk.Impact}) : {risk.Description}").ToArray();
        var marginFloor = study.ProductionCost * 1.25m;
        var marketCeiling = study.AverageMarketPrice > 0 ? study.AverageMarketPrice * 1.05m : study.ProposedSalePrice;
        var advisedPrice = marketCeiling >= marginFloor
            ? decimal.Clamp(study.ProposedSalePrice, marginFloor, marketCeiling) : marginFloor;
        advisedPrice = decimal.Round(advisedPrice, 2);
        var unitMargin = advisedPrice - study.ProductionCost;
        var initialExposure = Math.Max(study.ProductionCost, study.ProductionCost * study.MonthlySalesVolume);
        var minimumVolume = unitMargin <= 0 ? 0 : (int)decimal.Ceiling(initialExposure / unitMargin);

        return new CommercialRecommendationResult(recommendation, strengths, weaknesses,
            mainRisks, improvements, advisedPrice, minimumVolume);
    }

    private static void AddCriterion(decimal score, string strength, string weakness,
        string improvement, ICollection<string> strengths, ICollection<string> weaknesses,
        ICollection<string> improvements)
    {
        if (score >= 70m) strengths.Add(strength);
        if (score < 55m) { weaknesses.Add(weakness); improvements.Add(improvement); }
    }
}
