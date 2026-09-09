using Moq;
using ProductApp.Application.Products.Images;

namespace ProductApp.Application.Tests.Products;

public sealed class ProductImageTests
{
    [Fact]
    public async Task UploadHandler_AcceptsPngAndUsesSafeExtension()
    {
        var storage = new Mock<IProductImageStorage>();
        storage.Setup(item => item.SaveAsync(It.IsAny<Stream>(), ".png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/products/image.png");
        var handler = new UploadProductImageCommandHandler(storage.Object);
        await using var content = new MemoryStream([137, 80, 78, 71]);

        var result = await handler.Handle(
            new UploadProductImageCommand(content, "product.png", "image/png", content.Length),
            CancellationToken.None);

        Assert.Equal("/uploads/products/image.png", result);
        storage.Verify(item => item.SaveAsync(content, ".png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("image/svg+xml", 100)]
    [InlineData("image/png", 5242881)]
    public async Task UploadHandler_RejectsUnsupportedOrOversizedFiles(string contentType, long length)
    {
        var handler = new UploadProductImageCommandHandler(Mock.Of<IProductImageStorage>());
        await using var content = new MemoryStream([1]);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new UploadProductImageCommand(content, "image", contentType, length),
            CancellationToken.None));
    }
}
