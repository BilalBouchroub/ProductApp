using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Products;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class ProductVersionConfiguration : IEntityTypeConfiguration<ProductVersion>
{
    public void Configure(EntityTypeBuilder<ProductVersion> builder)
    {
        builder.ToTable("ProductVersions", table =>
        {
            table.HasCheckConstraint("CK_ProductVersions_Version", "\"VersionNumber\" > 0");
            table.HasCheckConstraint("CK_ProductVersions_Prices", "(\"TargetSalePrice\" IS NULL OR \"TargetSalePrice\" >= 0) AND (\"BatchQuantity\" IS NULL OR \"BatchQuantity\" > 0)");
        });
        builder.HasKey(x => x.Id); builder.HasAlternateKey(x => new { x.ProductId, x.VersionNumber });
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<int>(); builder.Property(x => x.TargetSalePrice).HasPrecision(18, 2);
        builder.Property(x => x.BatchQuantity).HasPrecision(18, 3); builder.Property(x => x.ProductionUnit).HasMaxLength(30);
        builder.Property(x => x.ChangeSummary).HasMaxLength(2000); builder.HasIndex(x => new { x.ProductId, x.Status });
        builder.HasOne(x => x.Product).WithMany(x => x.Versions).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class ProductionExperimentConfiguration : IEntityTypeConfiguration<ProductionExperiment>
{
    public void Configure(EntityTypeBuilder<ProductionExperiment> builder)
    {
        builder.ToTable("ProductionExperiments", table =>
        {
            table.HasCheckConstraint("CK_ProductionExperiments_Quantities", "\"PlannedQuantity\" > 0 AND (\"ActualQuantity\" IS NULL OR \"ActualQuantity\" >= 0)");
            table.HasCheckConstraint("CK_ProductionExperiments_Costs", "\"PlannedCost\" >= 0 AND (\"ActualCost\" IS NULL OR \"ActualCost\" >= 0)");
            table.HasCheckConstraint("CK_ProductionExperiments_Waste", "\"WasteRate\" IS NULL OR (\"WasteRate\" >= 0 AND \"WasteRate\" <= 100)");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Objective).HasMaxLength(2000).IsRequired(); builder.Property(x => x.Hypothesis).HasMaxLength(2000);
        builder.Property(x => x.StartDate).HasColumnType("datetime2"); builder.Property(x => x.EndDate).HasColumnType("datetime2");
        builder.Property(x => x.PlannedQuantity).HasPrecision(18, 3); builder.Property(x => x.ActualQuantity).HasPrecision(18, 3);
        builder.Property(x => x.PlannedCost).HasPrecision(18, 2); builder.Property(x => x.ActualCost).HasPrecision(18, 2);
        builder.Property(x => x.WasteRate).HasPrecision(5, 2); builder.Property(x => x.Result).HasConversion<int>();
        builder.Property(x => x.Observations).HasMaxLength(4000); builder.Property(x => x.Conclusion).HasMaxLength(4000);
        builder.HasIndex(x => new { x.ProductVersionId, x.StartDate }); builder.HasIndex(x => x.Result);
        builder.HasOne(x => x.ProductVersion).WithMany(x => x.Experiments).HasForeignKey(x => x.ProductVersionId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class ExperimentStepConfiguration : IEntityTypeConfiguration<ExperimentStep>
{
    public void Configure(EntityTypeBuilder<ExperimentStep> builder)
    {
        builder.ToTable("ExperimentSteps", table =>
        {
            table.HasCheckConstraint("CK_ExperimentSteps_Order", "\"Order\" > 0");
            table.HasCheckConstraint("CK_ExperimentSteps_Costs", "\"PlannedCost\" >= 0 AND (\"ActualCost\" IS NULL OR \"ActualCost\" >= 0)");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.PlannedCost).HasPrecision(18, 2);
        builder.Property(x => x.ActualCost).HasPrecision(18, 2); builder.Property(x => x.PlannedOutputQuantity).HasPrecision(18, 3);
        builder.Property(x => x.ActualOutputQuantity).HasPrecision(18, 3); builder.Property(x => x.Observations).HasMaxLength(4000);
        builder.HasIndex(x => new { x.ProductionExperimentId, x.Order }).IsUnique();
        builder.HasIndex(x => new { x.ProductionExperimentId, x.ProductionStepId }).IsUnique();
        builder.HasOne(x => x.ProductionExperiment).WithMany(x => x.Steps).HasForeignKey(x => x.ProductionExperimentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductionStep).WithMany().HasForeignKey(x => x.ProductionStepId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
