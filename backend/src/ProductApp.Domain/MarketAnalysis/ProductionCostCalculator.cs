namespace ProductApp.Domain.MarketAnalysis;

public sealed record MaterialCostLine(decimal UnitCost, decimal RequiredQuantity);

public sealed record ProductionCostResult(
    decimal MaterialCost,
    decimal LaborCost,
    decimal EquipmentCost,
    decimal TotalProductionCost);

public sealed class ProductionCostCalculator
{
    public ProductionCostResult Calculate(
        IEnumerable<MaterialCostLine> materials,
        decimal laborCost,
        decimal equipmentCost = 0)
    {
        ArgumentNullException.ThrowIfNull(materials);
        if (laborCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(laborCost));
        }

        if (equipmentCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(equipmentCost));
        }

        var materialCost = 0m;
        foreach (var material in materials)
        {
            if (material.UnitCost < 0 || material.RequiredQuantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(materials));
            }

            materialCost += material.UnitCost * material.RequiredQuantity;
        }

        materialCost = decimal.Round(materialCost, 2, MidpointRounding.AwayFromZero);
        laborCost = decimal.Round(laborCost, 2, MidpointRounding.AwayFromZero);
        equipmentCost = decimal.Round(equipmentCost, 2, MidpointRounding.AwayFromZero);

        return new ProductionCostResult(
            materialCost,
            laborCost,
            equipmentCost,
            materialCost + laborCost + equipmentCost);
    }
}
