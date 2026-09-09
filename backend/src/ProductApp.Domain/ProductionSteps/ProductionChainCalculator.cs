using ProductApp.Domain.Common;
using ProductApp.Domain.Resources;

namespace ProductApp.Domain.ProductionSteps;

public sealed record ProductionChainSummary(
    int TotalDurationMinutes,
    decimal TotalCost,
    int ResourceCount,
    int MachineCount,
    decimal EnergyConsumption);

public static class ProductionChainCalculator
{
    public static ProductionChainSummary Calculate(IEnumerable<ProductionStep> source)
    {
        var steps = source.Where(step => step.Status is RecordStatus.Active or RecordStatus.Validated).ToArray();
        var duplicateOrder = steps.GroupBy(step => step.Order).FirstOrDefault(group => group.Count() > 1);
        if (duplicateOrder is not null)
        {
            throw new InvalidOperationException($"Deux étapes ne peuvent pas avoir l’ordre {duplicateOrder.Key}.");
        }

        var resources = steps.SelectMany(step => step.Resources).ToArray();
        var resourceCosts = resources.Sum(resource => ResourceCalculator.Calculate(
            resource.PlannedQuantity, resource.ActualQuantity, resource.UnitCost, resource.AvailableStock).PlannedCost);

        return new ProductionChainSummary(
            steps.Sum(step => step.DurationMinutes),
            decimal.Round(steps.Sum(step => step.PlannedCost ?? step.LaborCost) + resourceCosts, 2),
            resources.Length,
            resources.Count(resource => resource.ResourceType == ResourceType.Machine),
            decimal.Round(steps.Sum(step => step.EnergyConsumption)
                + resources.Where(resource => resource.ResourceType == ResourceType.Energy)
                    .Sum(resource => resource.PlannedQuantity), 3));
    }
}
