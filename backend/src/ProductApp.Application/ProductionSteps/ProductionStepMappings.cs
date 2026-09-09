using ProductApp.Application.ProductionSteps.DTOs;
using ProductApp.Application.Production;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.ProductionSteps;

internal static class ProductionStepMappings
{
    public static ProductionStepDto ToDto(this ProductionStep step)
    {
        return new ProductionStepDto(
            step.Id,
            step.ProductId,
            step.Order,
            step.Name,
            step.Icon,
            step.Description,
            step.DurationMinutes,
            step.ActualDurationMinutes,
            step.PlannedCost,
            step.ActualCost,
            step.Temperature,
            step.Pressure,
            step.Humidity,
            step.PlannedOutputQuantity,
            step.ActualOutputQuantity,
            step.WasteQuantity,
            step.EquipmentName,
            step.OperatorCount,
            step.LaborCost,
            step.EnergyConsumption,
            step.Instructions,
            step.ValidationCriteria,
            step.Observations,
            step.Status,
            step.Resources.Select(resource => resource.ToDto()).ToArray());
    }
}
