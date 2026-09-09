using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Resources;

namespace ProductApp.Application.Production;

internal static class ProductionMappings
{
    public static ProductVersionDto ToDto(this ProductVersion version) => new(version.Id,
        version.ProductId, version.VersionNumber, version.Name, version.Description, version.Status,
        version.ChangeSummary, version.CreatedAt, version.UpdatedAt);

    public static ProductionChainSummaryDto ToDto(this ProductionChainSummary summary) => new(
        summary.TotalDurationMinutes, summary.TotalCost, summary.ResourceCount,
        summary.MachineCount, summary.EnergyConsumption);

    public static StepResourceDto ToDto(this StepResource resource)
    {
        var calculation = ResourceCalculator.Calculate(resource.PlannedQuantity,
            resource.ActualQuantity, resource.UnitCost, resource.AvailableStock);
        return new StepResourceDto(resource.Id, resource.ProductionStepId, resource.ResourceType,
            resource.Designation, resource.Unit, resource.PlannedQuantity, resource.ActualQuantity,
            resource.UnitCost, resource.TotalCost, resource.AvailableStock,
            calculation.RemainingStock, resource.AvailabilityStatus);
    }

    public static ProductionExperimentDto ToDto(this ProductionExperiment experiment)
    {
        ExperimentSummaryDto? summary = null;
        if (experiment.EndDate.HasValue && experiment.ActualQuantity.HasValue)
        {
            var calculation = ExperimentCalculator.Calculate(experiment.Steps,
                experiment.PlannedQuantity, experiment.ActualQuantity.Value);
            summary = calculation.ToDto();
        }
        return new ProductionExperimentDto(experiment.Id, experiment.ProductVersionId,
            experiment.Name, experiment.Objective, experiment.Hypothesis,
            experiment.StartDate, experiment.EndDate, experiment.PlannedQuantity,
            experiment.ActualQuantity, experiment.PlannedCost, experiment.ActualCost,
            experiment.PlannedDurationMinutes, experiment.ActualDurationMinutes,
            experiment.WasteRate, experiment.Result, experiment.Observations, experiment.Conclusion,
            experiment.Steps.OrderBy(step => step.Order).Select(step => new ExperimentStepDto(
                step.Id, step.ProductionStepId, step.Order, step.PlannedCost, step.ActualCost,
                step.PlannedDurationMinutes, step.ActualDurationMinutes,
                step.PlannedOutputQuantity, step.ActualOutputQuantity, step.IsValidated)).ToArray(), summary);
    }

    public static ExperimentSummaryDto ToDto(this ExperimentSummary summary) => new(
        summary.ActualCost, summary.ActualDurationMinutes, summary.CostVariance,
        summary.DurationVarianceMinutes, summary.WasteRate, summary.PerformanceScore,
        summary.StabilityScore, summary.Summary);
}
