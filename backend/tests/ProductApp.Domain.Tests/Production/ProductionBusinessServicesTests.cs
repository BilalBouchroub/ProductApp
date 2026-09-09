using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Resources;

namespace ProductApp.Domain.Tests.Production;

public sealed class ProductionBusinessServicesTests
{
    [Fact]
    public void ResourceCalculator_ComputesCostsStockAndAvailability()
    {
        var result = ResourceCalculator.Calculate(10m, 8m, 2.50m, 12m);

        Assert.Equal(25m, result.PlannedCost);
        Assert.Equal(20m, result.ActualCost);
        Assert.Equal(4m, result.RemainingStock);
        Assert.Equal(AvailabilityStatus.Available, result.Availability);
    }

    [Fact]
    public void ResourceCalculator_WhenStockIsInsufficient_ReturnsToOrder()
    {
        var result = ResourceCalculator.Calculate(10m, null, 1m, 6m);
        Assert.Equal(AvailabilityStatus.ToOrder, result.Availability);
        Assert.Equal(-4m, result.RemainingStock);
    }

    [Fact]
    public void ProductionChainCalculator_ComputesCompleteSummary()
    {
        var productId = Guid.NewGuid();
        var mixing = ProductionStep.CreateForVersion(productId, 1, 1, "Mélange", null, 10, 20, "Mélangeur", 15m);
        mixing.AddResource(StepResource.Create(mixing.Id, ResourceType.RawMaterial, "Farine", "kg", 10, null, 2m, 100m));
        mixing.AddResource(StepResource.Create(mixing.Id, ResourceType.Machine, "Mélangeur", "h", 1, null, 5m, null));
        var baking = ProductionStep.CreateForVersion(productId, 1, 2, "Cuisson", null, 20, 180, "Four", 25m);

        var summary = ProductionChainCalculator.Calculate([mixing, baking]);

        Assert.Equal(30, summary.TotalDurationMinutes);
        Assert.Equal(65m, summary.TotalCost);
        Assert.Equal(2, summary.ResourceCount);
        Assert.Equal(1, summary.MachineCount);
    }

    [Fact]
    public void ProductionChainCalculator_RejectsDuplicateOrders()
    {
        var productId = Guid.NewGuid();
        var first = ProductionStep.Create(productId, 1, "A", null, 10, 20, "M1", 1);
        var second = ProductionStep.Create(productId, 1, "B", null, 10, 20, "M2", 1);
        Assert.Throws<InvalidOperationException>(() => ProductionChainCalculator.Calculate([first, second]));
    }

    [Fact]
    public void ProductionChainCalculator_IncludesValidatedAndIgnoresInactiveSteps()
    {
        var productId = Guid.NewGuid();
        var validated = ProductionStep.Create(productId, 1, "Contrôle", null, 12, 20, "Poste", 5);
        validated.ConfigureDetails(null, null, null, null, null, null, null, null,
            1, 0, null, null, null, RecordStatus.Validated);
        var inactive = ProductionStep.Create(productId, 2, "Ancienne étape", null, 30, 20, "Poste", 5);
        inactive.ConfigureDetails(null, null, null, null, null, null, null, null,
            1, 0, null, null, null, RecordStatus.Inactive);

        var summary = ProductionChainCalculator.Calculate([validated, inactive]);

        Assert.Equal(12, summary.TotalDurationMinutes);
    }

    [Fact]
    public void CreateExperiment_WithMissingDuration_ReportsDurationParameter()
    {
        var plannedDurationMinutes = 0;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ProductionExperiment.Create(
            Guid.NewGuid(), "Essai", "Vérifier", null, DateTime.UtcNow, 100, 0,
            plannedDurationMinutes));

        Assert.Equal(nameof(plannedDurationMinutes), exception.ParamName);
    }

    [Fact]
    public void UpdateNarrative_StoresAllExperimentTextFields()
    {
        var experiment = ProductionExperiment.Create(Guid.NewGuid(), "Essai", "Objectif initial",
            null, DateTime.UtcNow, 100, 10, 20);

        experiment.UpdateNarrative("Objectif complet", "Hypothèse complète",
            "Observations complètes", "Conclusion complète");

        Assert.Equal("Objectif complet", experiment.Objective);
        Assert.Equal("Hypothèse complète", experiment.Hypothesis);
        Assert.Equal("Observations complètes", experiment.Observations);
        Assert.Equal("Conclusion complète", experiment.Conclusion);
    }

    [Fact]
    public void ExperimentCalculator_ComputesVariancesPerformanceAndStability()
    {
        var experimentId = Guid.NewGuid();
        var first = ExperimentStep.Create(experimentId, Guid.NewGuid(), 1, 100m, 60, 100m);
        first.RecordActuals(110m, 66, 92m, null, DateTime.UtcNow);
        var second = ExperimentStep.Create(experimentId, Guid.NewGuid(), 2, 50m, 30, 100m);
        second.RecordActuals(45m, 27, 92m, null, DateTime.UtcNow);

        var result = ExperimentCalculator.Calculate([first, second], 100m, 92m);

        Assert.Equal(155m, result.ActualCost);
        Assert.Equal(93, result.ActualDurationMinutes);
        Assert.Equal(5m, result.CostVariance);
        Assert.Equal(3, result.DurationVarianceMinutes);
        Assert.Equal(8m, result.WasteRate);
        Assert.InRange(result.PerformanceScore, 0m, 100m);
        Assert.InRange(result.StabilityScore, 0m, 100m);
        Assert.NotEmpty(result.Summary);
    }

    [Fact]
    public void ReadinessPolicy_RequiresChainAndExperiment()
    {
        var product = Product.Create("BIS-01", "Biscuit", null, DateTime.UtcNow);
        var result = ProductionReadinessPolicy.Evaluate(product.CurrentVersion);
        Assert.False(result.IsReady);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ReadinessPolicy_AcceptsCompleteProductionVersion()
    {
        var product = Product.Create("BIS-02", "Biscuit", null, DateTime.UtcNow);
        var version = product.CurrentVersion;
        var step = ProductionStep.CreateForVersion(product.Id, 1, 1, "Cuisson", null, 20, 180, "Four", 30m);
        version.ProductionSteps.Add(step);
        version.Experiments.Add(ProductionExperiment.Create(version.Id, "Essai", "Valider", null,
            DateTime.UtcNow, 100m, 30m, 20));

        var result = ProductionReadinessPolicy.Evaluate(version);
        Assert.True(result.IsReady);
        Assert.Empty(result.Errors);
    }
}
