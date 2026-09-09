namespace ProductApp.Domain.Experiments;

public sealed record ExperimentSummary(
    decimal ActualCost,
    int ActualDurationMinutes,
    decimal CostVariance,
    int DurationVarianceMinutes,
    decimal WasteRate,
    decimal PerformanceScore,
    decimal StabilityScore,
    string Summary);

public static class ExperimentCalculator
{
    public static ExperimentSummary Calculate(IEnumerable<ExperimentStep> source,
        decimal plannedQuantity, decimal actualQuantity)
    {
        var steps = source.ToArray();
        if (steps.Length == 0) throw new InvalidOperationException("L’expérience ne contient aucune étape.");
        if (plannedQuantity <= 0 || actualQuantity < 0) throw new ArgumentOutOfRangeException(nameof(plannedQuantity));

        var plannedCost = steps.Sum(step => step.PlannedCost);
        var actualCost = steps.Sum(step => step.ActualCost ?? 0m);
        var plannedDuration = steps.Sum(step => step.PlannedDurationMinutes);
        var actualDuration = steps.Sum(step => step.ActualDurationMinutes ?? 0);
        var costVariance = actualCost - plannedCost;
        var durationVariance = actualDuration - plannedDuration;
        var wasteRate = decimal.Round(Math.Max(0m, plannedQuantity - actualQuantity) / plannedQuantity * 100m, 2);
        var costScore = RatioScore(actualCost, plannedCost);
        var durationScore = RatioScore(actualDuration, plannedDuration);
        var yieldScore = decimal.Clamp(actualQuantity / plannedQuantity * 100m, 0m, 100m);
        var performance = decimal.Round((costScore + durationScore + yieldScore) / 3m, 2);

        var deviations = steps.Select(step =>
        {
            var cost = RelativeDeviation(step.ActualCost ?? 0m, step.PlannedCost);
            var duration = RelativeDeviation(step.ActualDurationMinutes ?? 0, step.PlannedDurationMinutes);
            return (cost + duration) / 2m;
        }).ToArray();
        var stability = decimal.Round(decimal.Clamp(100m - deviations.Average(), 0m, 100m), 2);
        var text = $"Coût réel {actualCost:F2}, durée réelle {actualDuration} min, perte {wasteRate:F2} %, performance {performance:F2}/100, stabilité {stability:F2}/100.";
        return new ExperimentSummary(decimal.Round(actualCost, 2), actualDuration,
            decimal.Round(costVariance, 2), durationVariance, wasteRate, performance, stability, text);
    }

    private static decimal RatioScore(decimal actual, decimal planned) => planned <= 0
        ? (actual <= 0 ? 100m : 0m)
        : decimal.Clamp(100m - Math.Max(0m, actual - planned) / planned * 100m, 0m, 100m);

    private static decimal RelativeDeviation(decimal actual, decimal planned) => planned == 0
        ? (actual == 0 ? 0m : 100m)
        : Math.Abs(actual - planned) / planned * 100m;
}
