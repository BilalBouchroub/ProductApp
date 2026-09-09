namespace ProductApp.Domain.Commercial;

public sealed class FinancialScoreCalculator
{
    public decimal Calculate(MarketStudy study)
    {
        ArgumentNullException.ThrowIfNull(study);
        if (study.ProposedSalePrice <= 0) return 0m;
        var marginRate = study.ProductionCost == 0 ? 100m
            : (study.ProposedSalePrice - study.ProductionCost) / study.ProductionCost * 100m;
        var margin = decimal.Clamp(marginRate / 40m * 100m, 0m, 100m);
        var positioning = study.AverageMarketPrice <= 0 ? 60m
            : decimal.Clamp(100m - Math.Abs(study.ProposedSalePrice - study.AverageMarketPrice)
                / study.AverageMarketPrice * 100m, 0m, 100m);
        var revenue = decimal.Clamp(study.AnnualRevenue / 1_000_000m * 100m, 0m, 100m);
        var volume = decimal.Clamp(study.MonthlySalesVolume / 10_000m * 100m, 0m, 100m);
        return decimal.Round(decimal.Clamp(margin * .40m + positioning * .20m
            + revenue * .25m + volume * .15m, 0m, 100m), 2);
    }
}
