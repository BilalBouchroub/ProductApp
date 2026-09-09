using ProductApp.Domain.Identity;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Domain.Tests.Products;

public sealed class ProductTechnicalDataTests
{
    [Fact]
    public void ConfigureTechnicalData_UpdatesProductAndCurrentVersion()
    {
        var now = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("BIS-001", "Biscuit", "Description", now);

        product.ConfigureTechnicalData("sap-100", "https://example.test/biscuit.png",
            18.50m, 1_000m, "kg", categoryId, now.AddMinutes(1));

        Assert.Equal("SAP-100", product.SapCode);
        Assert.Equal(18.50m, product.TargetSalePrice);
        Assert.Equal(1_000m, product.BatchQuantity);
        Assert.Equal("kg", product.ProductionUnit);
        Assert.Equal(categoryId, product.ProductCategoryId);
        Assert.Equal(18.50m, product.CurrentVersion.TargetSalePrice);
        Assert.Equal(1_000m, product.CurrentVersion.BatchQuantity);
    }

    [Fact]
    public void ConfigureTechnicalData_WithInvalidBatch_Throws()
    {
        var product = Product.Create("BIS-002", "Biscuit", null, DateTime.UtcNow);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ConfigureTechnicalData(
            null, null, 10m, 0m, "kg", null, DateTime.UtcNow));
    }

    [Fact]
    public void NotificationCreate_ProducesUnreadNotification()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var notification = Notification.Create(userId, "Produit prêt", "Une étude peut commencer.",
            NotificationType.Information, nameof(Product), productId, DateTime.UtcNow);

        Assert.Equal(userId, notification.UserId);
        Assert.Equal(productId, notification.RelatedEntityId);
        Assert.False(notification.IsRead);
    }
}
