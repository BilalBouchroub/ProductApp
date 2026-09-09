using Moq;
using ProductApp.Application.Products;
using ProductApp.Application.Products.Commands.CreateProduct;
using ProductApp.Application.Products.Commands.UpdateProduct;
using ProductApp.Application.Products.Queries.GetProductById;
using ProductApp.Domain.Products;

namespace ProductApp.Application.Tests.Products;

public sealed class ProductHandlersTests
{
    [Fact]
    public async Task CreateHandler_AddsProductAndSavesChanges()
    {
        var repository = new Mock<IProductRepository>();
        Product? addedProduct = null;
        repository.Setup(item => item.CodeExistsAsync("BISCUIT-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository.Setup(item => item.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => addedProduct = product)
            .Returns(Task.CompletedTask);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new CreateProductCommandHandler(repository.Object);

        var result = await handler.Handle(
            new CreateProductCommand("biscuit-01", "Biscuit", null),
            CancellationToken.None);

        Assert.NotNull(addedProduct);
        Assert.Equal("BISCUIT-01", result.Code);
        Assert.Equal(1, result.VersionNumber);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateHandler_WhenCodeExists_ThrowsConflict()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.CodeExistsAsync("BISCUIT-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateProductCommandHandler(repository.Object);

        await Assert.ThrowsAsync<ProductCodeConflictException>(() => handler.Handle(
            new CreateProductCommand("BISCUIT-01", "Biscuit", null),
            CancellationToken.None));
    }

    [Fact]
    public async Task UpdateHandler_IncrementsVersion()
    {
        var repository = new Mock<IProductRepository>();
        var product = Product.Create("BISCUIT-01", "Biscuit", null, DateTime.UtcNow);
        repository.Setup(item => item.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new UpdateProductCommandHandler(repository.Object);

        var result = await handler.Handle(
            new UpdateProductCommand(product.Id, "Updated Biscuit", null),
            CancellationToken.None);

        Assert.Equal(2, result.VersionNumber);
        Assert.Equal("Updated Biscuit", result.Name);
    }

    [Fact]
    public async Task GetByIdHandler_WhenMissing_ThrowsNotFound()
    {
        var repository = new Mock<IProductRepository>();
        var id = Guid.NewGuid();
        repository.Setup(item => item.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        var handler = new GetProductByIdQueryHandler(repository.Object);

        await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            handler.Handle(new GetProductByIdQuery(id), CancellationToken.None));
    }

    [Fact]
    public void CreateValidator_RejectsInvalidCodeAndName()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(new CreateProductCommand("invalid code", string.Empty, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Code));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void CreateValidator_RejectsInvalidThemeColor()
    {
        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(new CreateProductCommand(
            "BISCUIT-01", "Biscuit", null, ThemeColor: "blue"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.ThemeColor));
    }
}
