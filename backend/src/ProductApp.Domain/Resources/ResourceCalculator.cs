using ProductApp.Domain.Common;

namespace ProductApp.Domain.Resources;

public sealed record ResourceCalculation(
    decimal PlannedCost,
    decimal ActualCost,
    decimal? RemainingStock,
    AvailabilityStatus Availability);

public static class ResourceCalculator
{
    public static ResourceCalculation Calculate(decimal plannedQuantity, decimal? actualQuantity,
        decimal unitCost, decimal? availableStock)
    {
        if (plannedQuantity < 0 || actualQuantity < 0 || unitCost < 0 || availableStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(plannedQuantity), "Les quantités, coûts et stocks doivent être positifs.");
        }

        var consumed = actualQuantity ?? plannedQuantity;
        var required = Math.Max(plannedQuantity, consumed);
        decimal? remaining = availableStock.HasValue ? availableStock.Value - consumed : null;
        var availability = !availableStock.HasValue ? AvailabilityStatus.Available
            : availableStock.Value <= 0 ? AvailabilityStatus.Unavailable
            : availableStock.Value < required ? AvailabilityStatus.ToOrder
            : remaining <= plannedQuantity * 0.2m ? AvailabilityStatus.LowStock
            : AvailabilityStatus.Available;

        return new ResourceCalculation(
            decimal.Round(plannedQuantity * unitCost, 2),
            decimal.Round(consumed * unitCost, 2),
            remaining,
            availability);
    }
}
