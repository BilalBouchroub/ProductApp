using ProductApp.Infrastructure.SapIntegration;

namespace ProductApp.Infrastructure.Tests.SapIntegration;

public sealed class MockSapDataProviderTests
{
    private readonly MockSapDataProvider _provider = new();

    [Fact]
    public async Task GetMaterials_ReturnsExpectedBiscuitRawMaterials()
    {
        var materials = await _provider.GetMaterialsAsync(CancellationToken.None);

        Assert.Equal(5, materials.Count);
        Assert.Equal(
            [
                "MAT-FARINE-001",
                "MAT-SUCRE-001",
                "MAT-BEURRE-001",
                "MAT-CHOCOLAT-001",
                "MAT-EMBALLAGE-001"
            ],
            materials.Select(material => material.SapCode));
        Assert.All(materials, material =>
        {
            Assert.False(string.IsNullOrWhiteSpace(material.Designation));
            Assert.False(string.IsNullOrWhiteSpace(material.Unit));
            Assert.True(material.UnitCost > 0);
            Assert.True(material.AvailableQuantity >= 0);
        });
    }

    [Fact]
    public async Task GetBoms_ReturnsComponentsThatReferenceKnownMaterials()
    {
        var materials = await _provider.GetMaterialsAsync(CancellationToken.None);
        var boms = await _provider.GetBomsAsync(CancellationToken.None);
        var materialCodes = materials.Select(material => material.SapCode).ToHashSet();

        var bom = Assert.Single(boms);
        Assert.Equal("BISCUIT-CHOC-001", bom.ProductCode);
        Assert.Equal(5, bom.Components.Count);
        Assert.All(bom.Components, component =>
        {
            Assert.Contains(component.MaterialCode, materialCodes);
            Assert.True(component.RequiredQuantity > 0);
        });
    }

    [Fact]
    public async Task GetProductionOrders_ReturnsDevelopmentOrders()
    {
        var orders = await _provider.GetProductionOrdersAsync(CancellationToken.None);

        Assert.Equal(2, orders.Count);
        Assert.All(orders, order =>
        {
            Assert.StartsWith("OF-2026-", order.OrderNumber);
            Assert.Equal("BISCUIT-CHOC-001", order.ProductCode);
            Assert.True(order.PlannedQuantity > 0);
            Assert.True(order.PlannedEndUtc > order.PlannedStartUtc);
        });
    }

    [Fact]
    public async Task Provider_ObservesCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _provider.GetMaterialsAsync(cancellationTokenSource.Token));
    }
}
