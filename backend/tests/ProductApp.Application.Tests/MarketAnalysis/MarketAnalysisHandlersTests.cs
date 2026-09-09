using Moq;
using ProductApp.Application.MarketAnalysis;
using ProductApp.Application.MarketAnalysis.Commands.RunMarketAnalysis;
using ProductApp.Application.MarketAnalysis.Queries.GetAnalysisHistory;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps;
using ProductApp.Application.SapIntegration;
using ProductApp.Application.SapIntegration.DTOs;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Application.Tests.MarketAnalysis;

public sealed class MarketAnalysisHandlersTests
{
    [Fact]
    public async Task RunHandler_CalculatesAndPersistsCompleteAnalysis()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var steps = new[]
        {
            ProductionStep.Create(product.Id, 1, "Mixing", null, 10, 20m, "Mixer", 10m),
            ProductionStep.Create(product.Id, 2, "Baking", null, 20, 180m, "Oven", 20m)
        };
        var productRepository = CreateProductRepository(product);
        var stepRepository = new Mock<IProductionStepRepository>();
        var sapProvider = new Mock<ISapDataProvider>();
        var analysisRepository = new Mock<IMarketAnalysisRepository>();
        MarketAnalysisEntity? savedAnalysis = null;
        stepRepository.Setup(item => item.ListAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);
        sapProvider.Setup(item => item.GetMaterialsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new SapMaterialDto("FLOUR", "Flour", "KG", 0.85m, 100m),
                new SapMaterialDto("SUGAR", "Sugar", "KG", 1.10m, 100m)
            ]);
        analysisRepository.Setup(item => item.AddAsync(
                It.IsAny<MarketAnalysisEntity>(), It.IsAny<CancellationToken>()))
            .Callback<MarketAnalysisEntity, CancellationToken>((analysis, _) => savedAnalysis = analysis)
            .Returns(Task.CompletedTask);
        analysisRepository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = CreateHandler(
            analysisRepository.Object,
            productRepository.Object,
            stepRepository.Object,
            sapProvider.Object);

        var result = await handler.Handle(
            new RunMarketAnalysisCommand(
                product.Id,
                100m,
                6m,
                [new MaterialRequirement("FLOUR", 10m), new MaterialRequirement("SUGAR", 5m)]),
            CancellationToken.None);

        Assert.NotNull(savedAnalysis);
        Assert.Equal(14m, result.MaterialCost);
        Assert.Equal(30m, result.LaborCost);
        Assert.Equal(6m, result.EquipmentCost);
        Assert.Equal(50m, result.TotalProductionCost);
        Assert.Equal(50m, result.GrossMargin);
        Assert.Equal(50m, result.MarginRate);
        Assert.Equal(30, result.TotalCycleTimeMinutes);
        Assert.Equal(100m, result.FeasibilityScore);
        Assert.Equal(MarketRecommendation.Viable, result.Recommendation);
    }

    [Fact]
    public async Task RunHandler_WithoutProductionSteps_ThrowsPrerequisiteException()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var productRepository = CreateProductRepository(product);
        var stepRepository = new Mock<IProductionStepRepository>();
        stepRepository.Setup(item => item.ListAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = CreateHandler(
            Mock.Of<IMarketAnalysisRepository>(),
            productRepository.Object,
            stepRepository.Object,
            Mock.Of<ISapDataProvider>());

        await Assert.ThrowsAsync<MarketAnalysisPrerequisiteException>(() => handler.Handle(
            new RunMarketAnalysisCommand(
                product.Id, 100m, 0m, [new MaterialRequirement("FLOUR", 1m)]),
            CancellationToken.None));
    }

    [Fact]
    public async Task RunHandler_WithUnknownSapMaterial_Throws()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var productRepository = CreateProductRepository(product);
        var stepRepository = new Mock<IProductionStepRepository>();
        var sapProvider = new Mock<ISapDataProvider>();
        stepRepository.Setup(item => item.ListAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                ProductionStep.Create(product.Id, 1, "Mixing", null, 10, 20m, "Mixer", 10m)
            ]);
        sapProvider.Setup(item => item.GetMaterialsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = CreateHandler(
            Mock.Of<IMarketAnalysisRepository>(),
            productRepository.Object,
            stepRepository.Object,
            sapProvider.Object);

        await Assert.ThrowsAsync<SapMaterialNotFoundException>(() => handler.Handle(
            new RunMarketAnalysisCommand(
                product.Id, 100m, 0m, [new MaterialRequirement("UNKNOWN", 1m)]),
            CancellationToken.None));
    }

    [Fact]
    public async Task GetHistoryHandler_ReturnsRepositoryHistory()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var productRepository = CreateProductRepository(product);
        var analysisRepository = new Mock<IMarketAnalysisRepository>();
        var cost = new ProductionCostResult(10m, 5m, 0m, 15m);
        var margin = new MarginResult(5m, 25m);
        var analysis = MarketAnalysisEntity.Create(
            product.Id, cost, 20m, margin, 30, 1m, 80m,
            MarketRecommendation.Viable, DateTime.UtcNow);
        analysisRepository.Setup(item => item.ListByProductAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([analysis]);
        var handler = new GetAnalysisHistoryQueryHandler(
            analysisRepository.Object, productRepository.Object);

        var result = await handler.Handle(
            new GetAnalysisHistoryQuery(product.Id), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(analysis.Id, item.Id);
        Assert.Equal(MarketRecommendation.Viable, item.Recommendation);
    }

    [Fact]
    public void RunValidator_RejectsDuplicateMaterialsAndInvalidPrices()
    {
        var validator = new RunMarketAnalysisCommandValidator();

        var result = validator.Validate(new RunMarketAnalysisCommand(
            Guid.Empty,
            0m,
            -1m,
            [new MaterialRequirement("FLOUR", 1m), new MaterialRequirement("flour", 2m)]));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RunMarketAnalysisCommand.Materials));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RunMarketAnalysisCommand.TargetSellingPrice));
    }

    private static Mock<IProductRepository> CreateProductRepository(Product product)
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        return repository;
    }

    private static RunMarketAnalysisCommandHandler CreateHandler(
        IMarketAnalysisRepository analysisRepository,
        IProductRepository productRepository,
        IProductionStepRepository stepRepository,
        ISapDataProvider sapProvider)
    {
        return new RunMarketAnalysisCommandHandler(
            analysisRepository,
            productRepository,
            stepRepository,
            sapProvider,
            new ProductionCostCalculator(),
            new MarginCalculator(),
            new FeasibilityScoreCalculator(),
            new RecommendationEngine());
    }
}
