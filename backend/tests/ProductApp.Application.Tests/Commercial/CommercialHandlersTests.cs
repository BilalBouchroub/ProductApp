using Moq;
using ProductApp.Application.Commercial;
using ProductApp.Application.Commercial.Commands;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Tests.Commercial;

public sealed class CommercialHandlersTests
{
    [Fact]
    public async Task CreateStudyHandler_CreatesDraftAndStartsCommercialWorkflow()
    {
        var version = CreateReadyVersion("APP-COM-01");
        var repository = new Mock<ICommercialRepository>();
        MarketStudy? added = null;
        repository.Setup(x => x.GetProductVersionAsync(version.Id, It.IsAny<CancellationToken>())).ReturnsAsync(version);
        repository.Setup(x => x.AddStudyAsync(It.IsAny<MarketStudy>(), It.IsAny<CancellationToken>()))
            .Callback<MarketStudy, CancellationToken>((study, _) => added = study).Returns(Task.CompletedTask);
        repository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateStudyCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateStudyCommand(version.Id, ValidValues()), CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(MarketStudyStatus.Draft, result.Status);
        Assert.Equal(ProductVersionStatus.UnderMarketStudy, version.Status);
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateStudyHandler_ComputesScoresAndUpdatesProductVersion()
    {
        var version = CreateReadyVersion("APP-COM-02");
        version.BeginMarketStudy(DateTime.UtcNow);
        var study = MarketStudy.Create(version, ValidValues().Name, ValidValues().TargetMarket,
            ValidValues().GeographicArea, ValidValues().CustomerSegment, ValidValues().StudyDate,
            ValidValues().EstimatedMarketSize, ValidValues().AnnualGrowthRate,
            ValidValues().ProductionCost, ValidValues().ProposedSalePrice,
            ValidValues().AverageMarketPrice, ValidValues().MonthlySalesVolume);
        var repository = new Mock<ICommercialRepository>();
        repository.Setup(x => x.GetStudyAsync(study.Id, It.IsAny<CancellationToken>())).ReturnsAsync(study);
        repository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new ValidateStudyCommandHandler(repository.Object,
            new ProductionScoreCalculator(), new MarketScoreCalculator(),
            new FinancialScoreCalculator(), new RiskScoreCalculator(),
            new GlobalScoreCalculator(), new ProductApp.Domain.Commercial.RecommendationEngine());

        var result = await handler.Handle(new ValidateStudyCommand(study.Id), CancellationToken.None);

        Assert.Equal(MarketStudyStatus.Validated, study.Status);
        Assert.InRange(result.GlobalScore, 0m, 100m);
        Assert.Equal(result.Recommendation switch
        {
            Domain.MarketAnalysis.MarketRecommendation.Viable => ProductVersionStatus.Approved,
            Domain.MarketAnalysis.MarketRecommendation.ToOptimize => ProductVersionStatus.ToOptimize,
            _ => ProductVersionStatus.Rejected
        }, version.Status);
    }

    [Fact]
    public void CreateStudyValidator_RejectsInvalidFinancialValues()
    {
        var values = ValidValues() with { ProposedSalePrice = 0m, ProductionCost = -1m };
        var result = new CreateStudyCommandValidator().Validate(new CreateStudyCommand(Guid.NewGuid(), values));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RiskValidator_RejectsProbabilityOutsideScale()
    {
        var result = new RiskValuesValidator().Validate(new RiskValues("Financial", 6, 1, "Risque", null));
        Assert.False(result.IsValid);
    }

    private static ProductVersion CreateReadyVersion(string code)
    {
        var product = Product.Create(code, "Biscuit", null, DateTime.UtcNow);
        product.CurrentVersion.Publish(DateTime.UtcNow);
        return product.CurrentVersion;
    }

    private static MarketStudyValues ValidValues() => new("Étude", "Maroc", "National",
        "Familles", DateTime.UtcNow, 1_000_000m, 8m, 10m, 14m, 15m, 1_000m);
}
