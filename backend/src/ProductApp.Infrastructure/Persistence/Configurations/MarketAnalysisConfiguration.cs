using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Products;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class MarketAnalysisConfiguration : IEntityTypeConfiguration<MarketAnalysisEntity>
{
    public void Configure(EntityTypeBuilder<MarketAnalysisEntity> builder)
    {
        builder.ToTable("MarketAnalyses");
        builder.HasKey(analysis => analysis.Id);

        builder.Property(analysis => analysis.MaterialCost).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.LaborCost).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.EquipmentCost).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.TotalProductionCost).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.TargetSellingPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.GrossMargin).HasPrecision(18, 2).IsRequired();
        builder.Property(analysis => analysis.MarginRate).HasPrecision(9, 2).IsRequired();
        builder.Property(analysis => analysis.MaterialAvailabilityRate).HasPrecision(5, 4).IsRequired();
        builder.Property(analysis => analysis.FeasibilityScore).HasPrecision(5, 2).IsRequired();
        builder.Property(analysis => analysis.Recommendation).HasConversion<int>().IsRequired();
        builder.Property(analysis => analysis.CreatedAtUtc).HasColumnType("datetime2").IsRequired();

        builder.HasIndex(analysis => new { analysis.ProductId, analysis.CreatedAtUtc });
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(analysis => analysis.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
