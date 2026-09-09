using ProductApp.Domain.MarketAnalysis;

namespace ProductApp.Domain.Tests.MarketAnalysis;

public sealed class MarketAnalysisCalculatorsTests
{
    [Fact]
    public void ProductionCostCalculator_SumsAllCostCategories()
    {
        var calculator = new ProductionCostCalculator();

        var result = calculator.Calculate(
            [new MaterialCostLine(2.50m, 4m), new MaterialCostLine(1.25m, 2m)],
            laborCost: 12m,
            equipmentCost: 5.50m);

        Assert.Equal(12.50m, result.MaterialCost);
        Assert.Equal(12m, result.LaborCost);
        Assert.Equal(5.50m, result.EquipmentCost);
        Assert.Equal(30m, result.TotalProductionCost);
    }

    [Fact]
    public void ProductionCostCalculator_WithoutEquipment_UsesZero()
    {
        var calculator = new ProductionCostCalculator();

        var result = calculator.Calculate([new MaterialCostLine(2m, 3m)], 4m);

        Assert.Equal(0m, result.EquipmentCost);
        Assert.Equal(10m, result.TotalProductionCost);
    }

    [Fact]
    public void ProductionCostCalculator_WithNegativeCost_Throws()
    {
        var calculator = new ProductionCostCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            calculator.Calculate([new MaterialCostLine(-1m, 1m)], 0m));
    }

    [Theory]
    [InlineData(100, 70, 30, 30)]
    [InlineData(100, 120, -20, -20)]
    public void MarginCalculator_ComputesGrossMarginAndRate(
        decimal price,
        decimal cost,
        decimal expectedMargin,
        decimal expectedRate)
    {
        var calculator = new MarginCalculator();

        var result = calculator.Calculate(price, cost);

        Assert.Equal(expectedMargin, result.GrossMargin);
        Assert.Equal(expectedRate, result.MarginRate);
    }

    [Fact]
    public void MarginCalculator_WithZeroPrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MarginCalculator().Calculate(0m, 10m));
    }

    [Fact]
    public void FeasibilityScoreCalculator_WithIdealInputs_ReturnsOneHundred()
    {
        var score = new FeasibilityScoreCalculator().Calculate(30m, 1m, 480);

        Assert.Equal(100m, score);
    }

    [Fact]
    public void FeasibilityScoreCalculator_WithWorstInputs_ReturnsZero()
    {
        var score = new FeasibilityScoreCalculator().Calculate(-10m, 0m, 1440);

        Assert.Equal(0m, score);
    }

    [Fact]
    public void FeasibilityScoreCalculator_CombinesWeightedComponents()
    {
        var score = new FeasibilityScoreCalculator().Calculate(10m, 0.8m, 960);

        Assert.Equal(57.50m, score);
    }

    [Fact]
    public void FeasibilityScoreCalculator_WithInvalidAvailability_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FeasibilityScoreCalculator().Calculate(10m, 1.1m, 100));
    }

    [Theory]
    [InlineData(70, MarketRecommendation.Viable)]
    [InlineData(69.99, MarketRecommendation.ToOptimize)]
    [InlineData(40, MarketRecommendation.ToOptimize)]
    [InlineData(39.99, MarketRecommendation.NotViable)]
    public void RecommendationEngine_UsesScoreThresholds(
        decimal score,
        MarketRecommendation expected)
    {
        var recommendation = new RecommendationEngine().GetRecommendation(score);

        Assert.Equal(expected, recommendation);
    }

    [Fact]
    public void RecommendationEngine_WithScoreAboveOneHundred_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RecommendationEngine().GetRecommendation(100.01m));
    }
}
