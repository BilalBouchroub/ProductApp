using ProductApp.Domain.Catalog;
using ProductApp.Domain.Common;

namespace ProductApp.Domain.Products;

public sealed class Product : AuditableEntity
{
    private readonly List<ProductVersion> _versions = [];
    private Product()
    {
    }

    private Product(Guid id, string code, string name, string? description, DateTime createdAtUtc)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        VersionNumber = 1;
        Status = ProductStatus.Active;
        InitializeAudit(createdAtUtc);
        _versions.Add(ProductVersion.Create(id, 1, name, description, createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? SapCode { get; private set; }
    public string? ImageUrl { get; private set; }
    public string ThemeColor { get; private set; } = "#2563EB";
    public decimal? TargetSalePrice { get; private set; }
    public decimal? BatchQuantity { get; private set; }
    public string? ProductionUnit { get; private set; }
    public Guid? ProductCategoryId { get; private set; }
    public ProductCategory? ProductCategory { get; private set; }
    public IReadOnlyCollection<ProductVersion> Versions => _versions.AsReadOnly();
    public ProductVersion CurrentVersion => _versions.OrderByDescending(version => version.VersionNumber).First();

    public int VersionNumber { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTime CreatedAtUtc => CreatedAt;
    public DateTime UpdatedAtUtc => UpdatedAt;

    public static Product Create(string code, string name, string? description, DateTime createdAtUtc)
    {
        return new Product(Guid.NewGuid(), code, name, description, createdAtUtc);
    }

    public void Update(string name, string? description, DateTime updatedAtUtc)
    {
        EnsureActive();

        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeOptional(description);
        VersionNumber++;
        Touch(updatedAtUtc);
        _versions.Add(ProductVersion.Create(Id, VersionNumber, Name, Description, updatedAtUtc));
    }

    public void ConfigureTechnicalData(string? sapCode, string? imageUrl, decimal? targetSalePrice,
        decimal? batchQuantity, string? productionUnit, Guid? productCategoryId,
        DateTime updatedAtUtc, Guid? actorId = null)
    {
        EnsureActive();
        if (targetSalePrice is < 0) throw new ArgumentOutOfRangeException(nameof(targetSalePrice));
        if (batchQuantity is <= 0) throw new ArgumentOutOfRangeException(nameof(batchQuantity));
        SapCode = NormalizeOptional(sapCode)?.ToUpperInvariant();
        ImageUrl = NormalizeOptional(imageUrl);
        TargetSalePrice = targetSalePrice;
        BatchQuantity = batchQuantity;
        ProductionUnit = NormalizeOptional(productionUnit);
        ProductCategoryId = productCategoryId;
        CurrentVersion.ConfigureTechnicalData(targetSalePrice, batchQuantity, productionUnit, updatedAtUtc, actorId);
        Touch(updatedAtUtc, actorId);
    }

    public void Archive(DateTime archivedAtUtc)
    {
        EnsureActive();

        Status = ProductStatus.Archived;
        VersionNumber++;
        Touch(archivedAtUtc);
        _versions.Add(ProductVersion.Create(Id, VersionNumber, Name, Description, archivedAtUtc, ProductVersionStatus.Archived));
    }

    public void ConfigureTheme(string? themeColor, DateTime updatedAtUtc, Guid? actorId = null)
    {
        ThemeColor = NormalizeThemeColor(themeColor);
        Touch(updatedAtUtc, actorId);
    }

    public Product Duplicate(string newCode, DateTime createdAtUtc)
    {
        var duplicate = Create(newCode, Name, Description, createdAtUtc);
        duplicate.ConfigureTechnicalData(SapCode, ImageUrl, TargetSalePrice, BatchQuantity,
            ProductionUnit, ProductCategoryId, createdAtUtc);
        duplicate.ConfigureTheme(ThemeColor, createdAtUtc);
        return duplicate;
    }

    public ProductVersion CreateNewVersion(string? changeSummary, DateTime createdAtUtc, Guid? actorId = null)
    {
        EnsureActive();
        VersionNumber++;
        var version = ProductVersion.Create(Id, VersionNumber, Name, Description, createdAtUtc, changeSummary: changeSummary);
        _versions.Add(version);
        Touch(createdAtUtc, actorId);
        return version;
    }

    public bool CanBeDeleted() => Status == ProductStatus.Active && CurrentVersion.Status == ProductVersionStatus.Draft;

    public static string NormalizeCode(string code)
    {
        return NormalizeRequired(code, nameof(code)).ToUpperInvariant();
    }

    private void EnsureActive()
    {
        if (Status == ProductStatus.Archived)
        {
            throw new ProductArchivedException(Id);
        }
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeThemeColor(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "#2563EB" : value.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
