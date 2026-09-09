using ProductApp.Domain.Commercial;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Products;

public enum ProductVersionStatus
{
    Draft = 1, InConfiguration = 2, InExperiment = 3, ReadyForMarketStudy = 4,
    UnderMarketStudy = 5, Approved = 6, ToOptimize = 7, Rejected = 8, Archived = 9
}

public sealed class ProductVersion : AuditableEntity
{
    private ProductVersion() { }
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int VersionNumber { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductVersionStatus Status { get; private set; }
    public decimal? TargetSalePrice { get; private set; }
    public decimal? BatchQuantity { get; private set; }
    public string? ProductionUnit { get; private set; }
    public string? ChangeSummary { get; private set; }
    public ICollection<ProductionStep> ProductionSteps { get; private set; } = new List<ProductionStep>();
    public ICollection<ProductionExperiment> Experiments { get; private set; } = new List<ProductionExperiment>();
    public ICollection<MarketStudy> MarketStudies { get; private set; } = new List<MarketStudy>();

    internal static ProductVersion Create(Guid productId, int versionNumber, string name,
        string? description, DateTime createdAt,
        ProductVersionStatus status = ProductVersionStatus.Draft, string? changeSummary = null)
    {
        var version = new ProductVersion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VersionNumber = versionNumber,
            Name = name,
            Description = description,
            Status = status,
            ChangeSummary = changeSummary
        };
        version.InitializeAudit(createdAt);
        return version;
    }

    internal void ConfigureTechnicalData(decimal? targetSalePrice, decimal? batchQuantity,
        string? productionUnit, DateTime updatedAt, Guid? actorId = null)
    {
        if (targetSalePrice is < 0) throw new ArgumentOutOfRangeException(nameof(targetSalePrice));
        if (batchQuantity is <= 0) throw new ArgumentOutOfRangeException(nameof(batchQuantity));
        TargetSalePrice = targetSalePrice;
        BatchQuantity = batchQuantity;
        ProductionUnit = string.IsNullOrWhiteSpace(productionUnit) ? null : productionUnit.Trim();
        Touch(updatedAt, actorId);
    }

    public void Publish(DateTime publishedAt, Guid? actorId = null)
    {
        if (Status == ProductVersionStatus.Archived)
        {
            throw new InvalidOperationException("Une version archivée ne peut pas être publiée.");
        }

        Status = ProductVersionStatus.ReadyForMarketStudy;
        Touch(publishedAt, actorId);
    }

    public void BeginMarketStudy(DateTime updatedAt, Guid? actorId = null)
    {
        if (Status is not (ProductVersionStatus.ReadyForMarketStudy or ProductVersionStatus.ToOptimize
            or ProductVersionStatus.UnderMarketStudy))
            throw new InvalidOperationException("La version n’est pas disponible pour une étude commerciale.");
        Status = ProductVersionStatus.UnderMarketStudy;
        Touch(updatedAt, actorId);
    }

    public void ApplyRecommendation(MarketRecommendation recommendation, DateTime updatedAt,
        Guid? actorId = null)
    {
        Status = recommendation switch
        {
            MarketRecommendation.Viable => ProductVersionStatus.Approved,
            MarketRecommendation.ToOptimize => ProductVersionStatus.ToOptimize,
            _ => ProductVersionStatus.Rejected
        };
        Touch(updatedAt, actorId);
    }
}
