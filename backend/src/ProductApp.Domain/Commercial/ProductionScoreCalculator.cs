using ProductApp.Domain.Common;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Commercial;

public sealed class ProductionScoreCalculator
{
    public decimal Calculate(ProductVersion version, decimal productionCost, decimal proposedPrice)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (version.ProductionSteps.Count == 0) return 0m;
        var chain = ProductionChainCalculator.Calculate(version.ProductionSteps);
        var resources = version.ProductionSteps.SelectMany(step => step.Resources).ToArray();
        var complexity = Clamp(100m - Math.Abs(version.ProductionSteps.Count - 8) * 5m);
        var duration = Clamp(100m - Math.Max(0, chain.TotalDurationMinutes - 120) / 3m);
        var availability = resources.Length == 0 ? 70m : resources.Average(resource => resource.AvailabilityStatus switch
        {
            AvailabilityStatus.Available => 100m,
            AvailabilityStatus.LowStock => 65m,
            AvailabilityStatus.ToOrder => 35m,
            _ => 0m
        });
        var experiments = version.Experiments.Count == 0 ? 0m : version.Experiments.Average(experiment => experiment.Result switch
        {
            ExperimentResult.Success => 100m,
            ExperimentResult.PartialSuccess => 65m,
            ExperimentResult.Failed => 20m,
            _ => 0m
        });
        var averageWaste = version.Experiments.Where(experiment => experiment.WasteRate.HasValue)
            .Select(experiment => experiment.WasteRate!.Value).DefaultIfEmpty(20m).Average();
        var waste = Clamp(100m - averageWaste * 4m);
        var costEfficiency = proposedPrice <= 0 ? 0m : Clamp((proposedPrice - productionCost) / proposedPrice * 200m + 50m);
        return Round(complexity * .15m + duration * .15m + availability * .20m
            + experiments * .25m + waste * .15m + costEfficiency * .10m);
    }

    private static decimal Clamp(decimal value) => decimal.Clamp(value, 0m, 100m);
    private static decimal Round(decimal value) => decimal.Round(Clamp(value), 2);
}
