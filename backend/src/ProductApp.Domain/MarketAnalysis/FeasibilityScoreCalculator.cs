namespace ProductApp.Domain.MarketAnalysis;

public sealed class FeasibilityScoreCalculator
{
    private const decimal MaximumMarginPoints = 60m;
    private const decimal MaximumAvailabilityPoints = 25m;
    private const decimal MaximumCyclePoints = 15m;

    public decimal Calculate(
        decimal marginRate,
        decimal materialAvailabilityRate,
        int totalCycleTimeMinutes)
    {
        if (materialAvailabilityRate is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(materialAvailabilityRate));
        }

        if (totalCycleTimeMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCycleTimeMinutes));
        }

        var marginPoints = Math.Clamp(
            (marginRate + 10m) / 40m * MaximumMarginPoints,
            0m,
            MaximumMarginPoints);
        var availabilityPoints = materialAvailabilityRate * MaximumAvailabilityPoints;
        var cyclePoints = totalCycleTimeMinutes switch
        {
            <= 480 => MaximumCyclePoints,
            >= 1440 => 0m,
            _ => (1440m - totalCycleTimeMinutes) / 960m * MaximumCyclePoints
        };

        return decimal.Round(
            Math.Clamp(marginPoints + availabilityPoints + cyclePoints, 0m, 100m),
            2,
            MidpointRounding.AwayFromZero);
    }
}
