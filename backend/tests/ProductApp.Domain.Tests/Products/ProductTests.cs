using ProductApp.Domain.Products;

namespace ProductApp.Domain.Tests.Products;

public sealed class ProductTests
{
    private static readonly DateTime InitialDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_InitializesActiveProductAtVersionOne()
    {
        var product = Product.Create(" biscuit-01 ", "Biscuit", "Chocolate", InitialDate);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("BISCUIT-01", product.Code);
        Assert.Equal(1, product.VersionNumber);
        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Equal(InitialDate, product.CreatedAtUtc);
    }

    [Fact]
    public void Update_ChangesValuesAndIncrementsVersion()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, InitialDate);
        var updateDate = InitialDate.AddDays(1);

        product.Update("Premium Biscuit", "New recipe", updateDate);

        Assert.Equal("Premium Biscuit", product.Name);
        Assert.Equal("New recipe", product.Description);
        Assert.Equal(2, product.VersionNumber);
        Assert.Equal(updateDate, product.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_ChangesStatusAndIncrementsVersion()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, InitialDate);

        product.Archive(InitialDate.AddDays(1));

        Assert.Equal(ProductStatus.Archived, product.Status);
        Assert.Equal(2, product.VersionNumber);
    }

    [Fact]
    public void Update_WhenArchived_Throws()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, InitialDate);
        product.Archive(InitialDate.AddDays(1));

        Assert.Throws<ProductArchivedException>(() =>
            product.Update("Updated", null, InitialDate.AddDays(2)));
    }

    [Fact]
    public void Duplicate_CopiesContentWithNewIdentityCodeAndVersionOne()
    {
        var source = Product.Create("BISCUIT-01", "Biscuit", "Chocolate", InitialDate);
        source.Update("Premium Biscuit", "Dark chocolate", InitialDate.AddDays(1));

        var duplicate = source.Duplicate("BISCUIT-02", InitialDate.AddDays(2));

        Assert.NotEqual(source.Id, duplicate.Id);
        Assert.Equal("BISCUIT-02", duplicate.Code);
        Assert.Equal(source.Name, duplicate.Name);
        Assert.Equal(source.Description, duplicate.Description);
        Assert.Equal(1, duplicate.VersionNumber);
        Assert.Equal(ProductStatus.Active, duplicate.Status);
    }

    [Fact]
    public void ConfigureTheme_NormalizesColorAndDuplicateKeepsIt()
    {
        var product = Product.Create("BISCUIT-01", "Biscuit", null, InitialDate);
        product.ConfigureTheme("#a1b2c3", InitialDate.AddMinutes(1));

        var duplicate = product.Duplicate("BISCUIT-02", InitialDate.AddMinutes(2));

        Assert.Equal("#A1B2C3", product.ThemeColor);
        Assert.Equal(product.ThemeColor, duplicate.ThemeColor);
    }
}
