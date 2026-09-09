using Moq;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps;
using ProductApp.Application.ProductionSteps.Commands.CreateProductionStep;
using ProductApp.Application.ProductionSteps.Commands.ReorderProductionSteps;
using ProductApp.Application.ProductionSteps.Queries.GetProductionSteps;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.Tests.ProductionSteps;

public sealed class ProductionStepHandlersTests
{
    [Fact]
    public async Task CreateHandler_AddsStepForActiveProduct()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var productRepository = new Mock<IProductRepository>();
        var stepRepository = new Mock<IProductionStepRepository>();
        ProductionStep? addedStep = null;
        productRepository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        stepRepository.Setup(item => item.OrderExistsAsync(product.Id, 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        stepRepository.Setup(item => item.AddAsync(It.IsAny<ProductionStep>(), It.IsAny<CancellationToken>()))
            .Callback<ProductionStep, CancellationToken>((step, _) => addedStep = step)
            .Returns(Task.CompletedTask);
        stepRepository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new CreateProductionStepCommandHandler(stepRepository.Object, productRepository.Object);

        var result = await handler.Handle(
            new CreateProductionStepCommand(product.Id, 1, "Mixing", null, 15, 20m, "Mixer", 10m),
            CancellationToken.None);

        Assert.NotNull(addedStep);
        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal(1, result.Order);
    }

    [Fact]
    public async Task CreateHandler_WhenOrderExists_ThrowsConflict()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var productRepository = new Mock<IProductRepository>();
        var stepRepository = new Mock<IProductionStepRepository>();
        productRepository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        stepRepository.Setup(item => item.OrderExistsAsync(product.Id, 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateProductionStepCommandHandler(stepRepository.Object, productRepository.Object);

        await Assert.ThrowsAsync<ProductionStepOrderConflictException>(() => handler.Handle(
            new CreateProductionStepCommand(product.Id, 1, "Mixing", null, 15, 20m, "Mixer", 10m),
            CancellationToken.None));
    }

    [Fact]
    public async Task GetStepsHandler_ReturnsRepositoryOrder()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var first = ProductionStep.Create(product.Id, 1, "Mixing", null, 10, 20m, "Mixer", 10m);
        var second = ProductionStep.Create(product.Id, 2, "Baking", null, 20, 180m, "Oven", 20m);
        var productRepository = new Mock<IProductRepository>();
        var stepRepository = new Mock<IProductionStepRepository>();
        productRepository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        stepRepository.Setup(item => item.ListAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);
        var handler = new GetProductionStepsQueryHandler(stepRepository.Object, productRepository.Object);

        var result = await handler.Handle(new GetProductionStepsQuery(product.Id), CancellationToken.None);

        Assert.Equal([first.Id, second.Id], result.Select(step => step.Id));
    }

    [Fact]
    public async Task ReorderHandler_WithAllStepIds_CallsTransactionalReorder()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var first = ProductionStep.Create(product.Id, 1, "Mixing", null, 10, 20m, "Mixer", 10m);
        var second = ProductionStep.Create(product.Id, 2, "Baking", null, 20, 180m, "Oven", 20m);
        var productRepository = new Mock<IProductRepository>();
        var stepRepository = new Mock<IProductionStepRepository>();
        productRepository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        stepRepository.Setup(item => item.ListForUpdateAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);
        stepRepository.Setup(item => item.ReorderAsync(It.IsAny<IReadOnlyList<ProductionStep>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<ProductionStep>, CancellationToken>((steps, _) =>
            {
                for (var index = 0; index < steps.Count; index++)
                {
                    steps[index].ChangeOrder(index + 1);
                }
            })
            .Returns(Task.CompletedTask);
        var handler = new ReorderProductionStepsCommandHandler(stepRepository.Object, productRepository.Object);

        var result = await handler.Handle(
            new ReorderProductionStepsCommand(product.Id, [second.Id, first.Id]),
            CancellationToken.None);

        Assert.Equal([second.Id, first.Id], result.Select(step => step.Id));
        Assert.Equal([1, 2], result.Select(step => step.Order));
        stepRepository.Verify(item => item.ReorderAsync(
            It.IsAny<IReadOnlyList<ProductionStep>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReorderHandler_WithMissingStep_Throws()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        var step = ProductionStep.Create(product.Id, 1, "Mixing", null, 10, 20m, "Mixer", 10m);
        var productRepository = new Mock<IProductRepository>();
        var stepRepository = new Mock<IProductionStepRepository>();
        productRepository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        stepRepository.Setup(item => item.ListForUpdateAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([step]);
        var handler = new ReorderProductionStepsCommandHandler(stepRepository.Object, productRepository.Object);

        await Assert.ThrowsAsync<InvalidProductionStepOrderingException>(() => handler.Handle(
            new ReorderProductionStepsCommand(product.Id, [Guid.NewGuid()]),
            CancellationToken.None));
    }

    [Fact]
    public void CreateValidator_RejectsInvalidOperationalValues()
    {
        var validator = new CreateProductionStepCommandValidator();

        var result = validator.Validate(
            new CreateProductionStepCommand(Guid.Empty, 0, "", null, 0, -300m, "", -1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductionStepCommand.Order));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductionStepCommand.LaborCost));
    }
}
