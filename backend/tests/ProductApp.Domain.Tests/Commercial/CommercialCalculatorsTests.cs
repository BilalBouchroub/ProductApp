using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Tests.Commercial;

public sealed class CommercialCalculatorsTests
{
    [Fact]
    public void MarketStudy_CalculatesMarginRevenueAndLifecycle()
    {
        var version = CreateReadyVersion("COM-01");
        var study = CreateStudy(version);

        Assert.Equal(2m, study.CalculatedMargin);
        Assert.Equal(20m, study.MarginRate);
        Assert.Equal(144_000m, study.AnnualRevenue);
        Assert.Equal(MarketStudyStatus.Draft, study.Status);

        study.Update("Étude actualisée", "Maroc", "National", "Familles", DateTime.UtcNow,
            2_000_000m, 12m, 9m, 13m, 14m, 2_000m, DateTime.UtcNow);
        Assert.Equal(MarketStudyStatus.InProgress, study.Status);
        study.SaveAsDraft("Étude", "Maroc", "National", "Familles", DateTime.UtcNow,
            2_000_000m, 12m, 9m, 13m, 14m, 2_000m, DateTime.UtcNow);
        Assert.Equal(MarketStudyStatus.Draft, study.Status);
    }

    [Fact]
    public void RiskScore_HighExposureProducesLowerScore()
    {
        var studyId = Guid.NewGuid();
        var calculator = new RiskScoreCalculator();
        var low = calculator.Calculate([Risk.Create(studyId, "Supply", 1, 1, "Mineur", null)]);
        var high = calculator.Calculate([Risk.Create(studyId, "Financial", 5, 5, "Critique", "Réduire")]);
        Assert.True(low > high);
        Assert.Equal(0m, high);
    }

    [Fact]
    public void GlobalScore_UsesDocumentedWeights()
    {
        var result = new GlobalScoreCalculator().Calculate(80m, 70m, 60m, 50m);
        Assert.Equal(66.50m, result.GlobalScore);
    }

    [Fact]
    public void RecommendationEngine_ReturnsViableWithDetailedRecommendations()
    {
        var study = CreateStudy(CreateReadyVersion("COM-02"));
        var scores = new CommercialScoreResult(82m, 78m, 75m, 70m, 77.25m);
        var result = new ProductApp.Domain.Commercial.RecommendationEngine().Generate(study, scores);
        Assert.Equal(MarketRecommendation.Viable, result.Recommendation);
        Assert.NotEmpty(result.Strengths);
        Assert.True(result.AdvisedPrice > study.ProductionCost);
        Assert.True(result.MinimumVolume > 0);
    }

    [Fact]
    public void RecommendationEngine_ReturnsNotViableWhenCriticalScoresAreLow()
    {
        var study = CreateStudy(CreateReadyVersion("COM-03"));
        var result = new ProductApp.Domain.Commercial.RecommendationEngine().Generate(study,
            new CommercialScoreResult(40m, 35m, 20m, 15m, 29m));
        Assert.Equal(MarketRecommendation.NotViable, result.Recommendation);
        Assert.NotEmpty(result.Weaknesses);
        Assert.NotEmpty(result.Improvements);
    }

    [Fact]
    public void RecommendationEngine_ReturnsToOptimizeForPromisingButImperfectStudy()
    {
        var study = CreateStudy(CreateReadyVersion("COM-OPT"));
        var result = new ProductApp.Domain.Commercial.RecommendationEngine().Generate(study,
            new CommercialScoreResult(62m, 60m, 50m, 45m, 55.65m));
        Assert.Equal(MarketRecommendation.ToOptimize, result.Recommendation);
        Assert.NotEmpty(result.Improvements);
    }

    [Fact]
    public void Validate_MakesStudyImmutable()
    {
        var study = CreateStudy(CreateReadyVersion("COM-04"));
        var scores = new CommercialScoreResult(80, 80, 80, 80, 80);
        study.Validate(scores, MarketRecommendation.Viable, DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() => study.Update("X", "Y", "Z", "C",
            DateTime.UtcNow, 1, 1, 1, 2, 2, 1, DateTime.UtcNow));
    }

    [Fact]
    public void OptimizationRequest_RequiresValidatedStudyAndNormalizesChanges()
    {
        var study = CreateStudy(CreateReadyVersion("COM-REQ"));
        Assert.Throws<InvalidOperationException>(() => OptimizationRequest.Create(
            study, "Réduire les pertes", OptimizationPriority.High, ["Réduire les pertes"]));

        study.Validate(new CommercialScoreResult(60, 60, 55, 50, 57),
            MarketRecommendation.ToOptimize, DateTime.UtcNow);
        var request = OptimizationRequest.Create(study, " Réduire les pertes ",
            OptimizationPriority.High, [" Réduire les pertes ", "réduire les pertes"]);

        Assert.Equal(OptimizationStatus.New, request.Status);
        Assert.Equal("Réduire les pertes", request.Message);
        Assert.Single(request.RequestedChanges);
    }

    [Fact]
    public void AllCalculators_ReturnScoresBetweenZeroAndOneHundred()
    {
        var version = CreateReadyVersion("COM-05");
        var study = CreateStudy(version);
        study.Competitors.Add(Competitor.Create(study.Id, "Concurrent", "Biscuit", 13m,
            100m, 75m, 20m, "Marque", "Prix", "GMS", 4m));
        study.Risks.Add(Risk.Create(study.Id, "Competitive", 2, 3, "Concurrence", "Différencier"));
        var scores = new[]
        {
            new ProductionScoreCalculator().Calculate(version, study.ProductionCost, study.ProposedSalePrice),
            new MarketScoreCalculator().Calculate(study),
            new FinancialScoreCalculator().Calculate(study),
            new RiskScoreCalculator().Calculate(study.Risks)
        };
        Assert.All(scores, score => Assert.InRange(score, 0m, 100m));
    }

    private static ProductVersion CreateReadyVersion(string code)
    {
        var product = Product.Create(code, "Biscuit", null, DateTime.UtcNow);
        var version = product.CurrentVersion;
        version.ProductionSteps.Add(ProductionStep.CreateForVersion(product.Id, 1, 1,
            "Cuisson", null, 60, 180, "Four", 10m));
        var experiment = ProductionExperiment.Create(version.Id, "Essai", "Valider",
            null, DateTime.UtcNow, 100m, 10m, 60);
        var step = ExperimentStep.Create(experiment.Id, version.ProductionSteps.Single().Id,
            1, 10m, 60, 100m);
        step.RecordActuals(10m, 60, 98m, null, DateTime.UtcNow);
        experiment.AddStep(step);
        experiment.Complete(98m, ExperimentResult.Success, null, "Stable", DateTime.UtcNow);
        version.Experiments.Add(experiment);
        version.Publish(DateTime.UtcNow);
        return version;
    }

    private static MarketStudy CreateStudy(ProductVersion version) => MarketStudy.Create(version,
        "Étude biscuit", "Maroc", "National", "Familles", DateTime.UtcNow,
        1_500_000m, 10m, 10m, 12m, 13m, 1_000m);
}
