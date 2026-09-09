using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Tests.ProductionSteps;

public sealed class ProductionStepTests
{
    [Fact]
    public void Create_StoresAllValues()
    {
        var productId = Guid.NewGuid();

        var step = ProductionStep.Create(
            productId, 1, "Mixing", "Mix ingredients", 15, 22.5m, "Mixer", 125.50m);

        Assert.NotEqual(Guid.Empty, step.Id);
        Assert.Equal(productId, step.ProductId);
        Assert.Equal(1, step.Order);
        Assert.Equal("Mixing", step.Name);
        Assert.Equal(15, step.DurationMinutes);
        Assert.Equal(22.5m, step.Temperature);
        Assert.Equal("Mixer", step.EquipmentName);
        Assert.Equal(125.50m, step.LaborCost);
    }

    [Fact]
    public void Create_WithInvalidOrder_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ProductionStep.Create(
            Guid.NewGuid(), 0, "Mixing", null, 15, 20m, "Mixer", 10m));
    }

    [Fact]
    public void Update_ChangesOperationalValues()
    {
        var step = ProductionStep.Create(
            Guid.NewGuid(), 1, "Mixing", null, 15, 20m, "Mixer", 10m);

        step.Update(2, "Baking", "Bake product", 30, 180m, "Oven", 25m);

        Assert.Equal(2, step.Order);
        Assert.Equal("Baking", step.Name);
        Assert.Equal(30, step.DurationMinutes);
        Assert.Equal(180m, step.Temperature);
        Assert.Equal("Oven", step.EquipmentName);
        Assert.Equal(25m, step.LaborCost);
    }

    [Fact]
    public void ChangeOrder_WithPositiveOrder_UpdatesOrder()
    {
        var step = ProductionStep.Create(
            Guid.NewGuid(), 1, "Mixing", null, 15, 20m, "Mixer", 10m);

        step.ChangeOrder(3);

        Assert.Equal(3, step.Order);
    }

    [Fact]
    public void SetIcon_StoresSelectedIcon()
    {
        var step = ProductionStep.Create(
            Guid.NewGuid(), 1, nameof(SetIcon_StoresSelectedIcon), null, 15, 20m,
            nameof(ProductionStep), 10m);

        step.SetIcon(ProductionStepIconNames.Heating);

        Assert.Equal(ProductionStepIconNames.Heating, step.Icon);
    }
}
