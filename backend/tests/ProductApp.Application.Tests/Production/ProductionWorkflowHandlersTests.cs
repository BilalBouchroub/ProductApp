using Moq;
using ProductApp.Application.Production;
using ProductApp.Application.Production.Commands;
using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.Tests.Production;

public sealed class ProductionWorkflowHandlersTests
{
    [Fact]
    public async Task CreateVersionHandler_CreatesNextVersionAndSaves()
    {
        var product = Product.Create("BIS-10", "Biscuit", null, DateTime.UtcNow);
        var repository = new Mock<IProductionWorkflowRepository>();
        repository.Setup(x => x.GetProductAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        repository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateProductVersionCommandHandler(repository.Object);

        var result = await handler.Handle(new CreateProductVersionCommand(product.Id, "Nouvelle recette"), CancellationToken.None);

        Assert.Equal(2, result.VersionNumber);
        Assert.Equal("Nouvelle recette", result.ChangeSummary);
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishHandler_WhenVersionIsComplete_PublishesVersion()
    {
        var product = Product.Create("BIS-11", "Biscuit", null, DateTime.UtcNow);
        var version = product.CurrentVersion;
        version.ProductionSteps.Add(ProductionStep.CreateForVersion(product.Id, 1, 1,
            "Cuisson", null, 15, 180, "Four", 25m));
        version.Experiments.Add(ProductionExperiment.Create(version.Id, "Essai", "Validation",
            null, DateTime.UtcNow, 100, 25, 15));
        var repository = new Mock<IProductionWorkflowRepository>();
        repository.Setup(x => x.GetVersionAsync(product.Id, 1, It.IsAny<CancellationToken>())).ReturnsAsync(version);
        repository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new PublishProductCommandHandler(repository.Object);

        var result = await handler.Handle(new PublishProductCommand(product.Id, 1), CancellationToken.None);

        Assert.Equal(ProductVersionStatus.ReadyForMarketStudy, result.Status);
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishHandler_WhenPrerequisitesAreMissing_ThrowsWorkflowError()
    {
        var product = Product.Create("BIS-12", "Biscuit", null, DateTime.UtcNow);
        var repository = new Mock<IProductionWorkflowRepository>();
        repository.Setup(x => x.GetVersionAsync(product.Id, 1, It.IsAny<CancellationToken>())).ReturnsAsync(product.CurrentVersion);
        var handler = new PublishProductCommandHandler(repository.Object);

        var exception = await Assert.ThrowsAsync<ProductionWorkflowException>(() =>
            handler.Handle(new PublishProductCommand(product.Id, 1), CancellationToken.None));

        Assert.Contains("chaîne", exception.Message);
    }

    [Fact]
    public async Task CreateExperiment_WhenNoStepIsActiveOrValidated_ReturnsClearWorkflowError()
    {
        var product = Product.Create("BIS-13", "Biscuit", null, DateTime.UtcNow);
        var step = ProductionStep.CreateForVersion(product.Id, 1, 1,
            "Préparation", null, 15, 20, "Poste", 5);
        step.ConfigureDetails(null, null, null, null, null, null, null, null,
            1, 0, null, null, null, RecordStatus.Draft);
        product.CurrentVersion.ProductionSteps.Add(step);
        var repository = new Mock<IProductionWorkflowRepository>();
        repository.Setup(x => x.GetVersionAsync(product.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product.CurrentVersion);
        var handler = new CreateProductionExperimentCommandHandler(repository.Object);

        var exception = await Assert.ThrowsAsync<ProductionWorkflowException>(() => handler.Handle(
            new CreateProductionExperimentCommand(product.Id, 1, "Essai", "Valider la production",
                null, DateTime.UtcNow, 100), CancellationToken.None));

        Assert.Contains("Activez ou validez", exception.Message);
    }

    [Fact]
    public async Task UpdateExperimentNarrative_StoresTextAndSaves()
    {
        var experiment = ProductionExperiment.Create(Guid.NewGuid(), "Essai", "Objectif initial",
            null, DateTime.UtcNow, 100, 10, 20);
        var repository = new Mock<IProductionWorkflowRepository>();
        repository.Setup(x => x.GetExperimentAsync(experiment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(experiment);
        repository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new UpdateProductionExperimentNarrativeCommandHandler(repository.Object);

        var result = await handler.Handle(new UpdateProductionExperimentNarrativeCommand(
            experiment.Id, "Objectif complet", "Hypothèse complète",
            "Observations complètes", "Conclusion complète"), CancellationToken.None);

        Assert.Equal("Objectif complet", result.Objective);
        Assert.Equal("Hypothèse complète", result.Hypothesis);
        Assert.Equal("Observations complètes", result.Observations);
        Assert.Equal("Conclusion complète", result.Conclusion);
        repository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ResourceValidator_RejectsNegativeValues()
    {
        var validator = new AddStepResourceCommandValidator();
        var result = validator.Validate(new AddStepResourceCommand(Guid.NewGuid(),
            Domain.Common.ResourceType.RawMaterial, "Farine", "kg", -1, null, -2, -3, null, null));
        Assert.False(result.IsValid);
    }
}
