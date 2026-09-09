namespace ProductApp.Domain.Commercial;

public sealed class MarketScoreCalculator
{
    public decimal Calculate(MarketStudy study)
    {
        ArgumentNullException.ThrowIfNull(study);
        var size = decimal.Clamp(study.EstimatedMarketSize / 1_000_000m * 100m, 0m, 100m);
        var growth = decimal.Clamp((study.AnnualGrowthRate + 5m) / 25m * 100m, 0m, 100m);
        var competitorCount = study.Competitors.Count;
        var averageShare = study.Competitors.Where(x => x.MarketShare.HasValue)
            .Select(x => x.MarketShare!.Value).DefaultIfEmpty(0m).Average();
        var competitiveSpace = decimal.Clamp(100m - competitorCount * 7m - averageShare * .5m, 0m, 100m);
        var customerSignal = study.Competitors.Where(x => x.CustomerRating.HasValue)
            .Select(x => x.CustomerRating!.Value * 20m).DefaultIfEmpty(60m).Average();
        var definition = string.IsNullOrWhiteSpace(study.TargetMarket)
            || string.IsNullOrWhiteSpace(study.CustomerSegment) ? 30m : 100m;
        return decimal.Round(decimal.Clamp(size * .25m + growth * .25m + competitiveSpace * .25m
            + customerSignal * .15m + definition * .10m, 0m, 100m), 2);
    }
}
