using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Commercial;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class MarketStudyConfiguration : IEntityTypeConfiguration<MarketStudy>
{
    public void Configure(EntityTypeBuilder<MarketStudy> builder)
    {
        builder.ToTable("MarketStudies", table =>
        {
            table.HasCheckConstraint("CK_MarketStudies_Scores", "\"ProductionScore\" BETWEEN 0 AND 100 AND \"MarketScore\" BETWEEN 0 AND 100 AND \"FinancialScore\" BETWEEN 0 AND 100 AND \"RiskScore\" BETWEEN 0 AND 100 AND \"GlobalScore\" BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_MarketStudies_Financials", "\"ProductionCost\" >= 0 AND \"ProposedSalePrice\" >= 0 AND \"AverageMarketPrice\" >= 0");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetMarket).HasMaxLength(250).IsRequired(); builder.Property(x => x.GeographicArea).HasMaxLength(250).IsRequired();
        builder.Property(x => x.CustomerSegment).HasMaxLength(500).IsRequired(); builder.Property(x => x.StudyDate).HasColumnType("date");
        builder.Property(x => x.Status).HasConversion<int>(); builder.Property(x => x.Recommendation).HasConversion<int>();
        builder.Property(x => x.EstimatedMarketSize).HasPrecision(18, 2); builder.Property(x => x.AnnualGrowthRate).HasPrecision(7, 2);
        builder.Property(x => x.ProductionCost).HasPrecision(18, 2); builder.Property(x => x.ProposedSalePrice).HasPrecision(18, 2);
        builder.Property(x => x.AverageMarketPrice).HasPrecision(18, 2); builder.Property(x => x.CalculatedMargin).HasPrecision(18, 2);
        builder.Property(x => x.MarginRate).HasPrecision(7, 2); builder.Property(x => x.MonthlySalesVolume).HasPrecision(18, 3);
        builder.Property(x => x.AnnualRevenue).HasPrecision(18, 2); builder.Property(x => x.ProductionScore).HasPrecision(5, 2);
        builder.Property(x => x.MarketScore).HasPrecision(5, 2); builder.Property(x => x.FinancialScore).HasPrecision(5, 2);
        builder.Property(x => x.RiskScore).HasPrecision(5, 2); builder.Property(x => x.GlobalScore).HasPrecision(5, 2);
        builder.HasIndex(x => new { x.ProductVersionId, x.StudyDate }); builder.HasIndex(x => new { x.Status, x.Recommendation });
        builder.HasOne(x => x.ProductVersion).WithMany(x => x.MarketStudies).HasForeignKey(x => x.ProductVersionId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class CompetitorConfiguration : IEntityTypeConfiguration<Competitor>
{
    public void Configure(EntityTypeBuilder<Competitor> builder)
    {
        builder.ToTable("Competitors", table =>
        {
            table.HasCheckConstraint("CK_Competitors_PriceQuantity", "\"Price\" >= 0 AND \"Quantity\" > 0");
            table.HasCheckConstraint("CK_Competitors_Scores", "(\"EstimatedQualityScore\" IS NULL OR \"EstimatedQualityScore\" BETWEEN 0 AND 100) AND (\"MarketShare\" IS NULL OR \"MarketShare\" BETWEEN 0 AND 100) AND (\"CustomerRating\" IS NULL OR \"CustomerRating\" BETWEEN 0 AND 5)");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired(); builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Quantity).HasPrecision(18, 3); builder.Property(x => x.EstimatedQualityScore).HasPrecision(5, 2);
        builder.Property(x => x.MarketShare).HasPrecision(5, 2); builder.Property(x => x.CustomerRating).HasPrecision(3, 2);
        builder.Property(x => x.Strengths).HasMaxLength(2000); builder.Property(x => x.Weaknesses).HasMaxLength(2000);
        builder.Property(x => x.SalesChannels).HasMaxLength(1000); builder.HasIndex(x => new { x.MarketStudyId, x.Name });
        builder.HasOne(x => x.MarketStudy).WithMany(x => x.Competitors).HasForeignKey(x => x.MarketStudyId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class RiskConfiguration : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks", table =>
        {
            table.HasCheckConstraint("CK_Risks_Probability", "\"Probability\" BETWEEN 1 AND 5");
            table.HasCheckConstraint("CK_Risks_Impact", "\"Impact\" BETWEEN 1 AND 5");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.RiskType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired(); builder.Property(x => x.MitigationAction).HasMaxLength(2000);
        builder.HasIndex(x => new { x.MarketStudyId, x.Probability, x.Impact });
        builder.HasOne(x => x.MarketStudy).WithMany(x => x.Risks).HasForeignKey(x => x.MarketStudyId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class OptimizationRequestConfiguration : IEntityTypeConfiguration<OptimizationRequest>
{
    public void Configure(EntityTypeBuilder<OptimizationRequest> builder)
    {
        builder.ToTable("OptimizationRequests"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired(); builder.Property(x => x.Priority).HasConversion<int>();
        builder.Property(x => x.RequestedChangesJson).HasColumnType("nvarchar(max)").IsRequired(); builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.ResolvedAt).HasColumnType("datetime2"); builder.Property(x => x.Resolution).HasMaxLength(4000);
        builder.HasIndex(x => new { x.MarketStudyId, x.Status }); builder.HasIndex(x => new { x.Priority, x.CreatedAt });
        builder.HasOne(x => x.MarketStudy).WithMany(x => x.OptimizationRequests).HasForeignKey(x => x.MarketStudyId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
