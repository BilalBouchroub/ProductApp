namespace ProductApp.Domain.MarketAnalysis;

public sealed record MarginResult(decimal GrossMargin, decimal MarginRate);

public sealed class MarginCalculator
{
    public MarginResult Calculate(decimal targetSellingPrice, decimal totalProductionCost)
    {
        if (targetSellingPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSellingPrice));
        }

        if (totalProductionCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalProductionCost));
        }

        var grossMargin = targetSellingPrice - totalProductionCost;
        var marginRate = grossMargin / targetSellingPrice * 100m;

        return new MarginResult(
            decimal.Round(grossMargin, 2, MidpointRounding.AwayFromZero),
            decimal.Round(marginRate, 2, MidpointRounding.AwayFromZero));
    }
}
