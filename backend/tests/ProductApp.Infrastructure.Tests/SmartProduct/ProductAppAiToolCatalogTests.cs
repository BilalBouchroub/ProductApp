using ProductApp.Infrastructure.SmartProduct;

namespace ProductApp.Infrastructure.Tests.SmartProduct;

public sealed class ProductAppAiToolCatalogTests
{
    [Theory]
    [InlineData("Voir les produits existants")]
    [InlineData("Liste des produits créés")]
    [InlineData("Quels produits sont disponibles ?")]
    [InlineData("Affiche-moi le catalogue")]
    [InlineData("Combien de produits existent dans ProductApp ?")]
    public void ProductCatalogIntent_IsRecognized(string prompt)
    {
        Assert.True(ProductAppAiToolCatalog.IsProductCatalogRequest(prompt));
    }

    [Theory]
    [InlineData("Analyse le produit P001")]
    [InlineData("Quel est le coût de P002 ?")]
    [InlineData("Compare EXP-21 et EXP-25")]
    public void ProductSpecificIntent_DoesNotRequestTheWholeCatalog(string prompt)
    {
        Assert.False(ProductAppAiToolCatalog.IsProductCatalogRequest(prompt));
    }
}
