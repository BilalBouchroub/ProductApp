using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Products;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class ProductionStepConfiguration : IEntityTypeConfiguration<ProductionStep>
{
    public void Configure(EntityTypeBuilder<ProductionStep> builder)
    {
        builder.ToTable("ProductionSteps");
        builder.HasKey(step => step.Id);

        builder.Property(step => step.Order).IsRequired();
        builder.HasIndex(step => new { step.ProductId, step.ProductVersionNumber, step.Order }).IsUnique();
        builder.Property(step => step.Name).HasMaxLength(200).IsRequired();
        builder.Property(step => step.Icon).HasMaxLength(50).HasDefaultValue(ProductionStepIconNames.Automatic).IsRequired();
        builder.Property(step => step.Description).HasMaxLength(2000);
        builder.Property(step => step.DurationMinutes).IsRequired();
        builder.Property(step => step.Temperature).HasPrecision(10, 2).IsRequired();
        builder.Property(step => step.EquipmentName).HasMaxLength(200).IsRequired();
        builder.Property(step => step.LaborCost).HasPrecision(18, 2).IsRequired();
        builder.Property(step => step.PlannedCost).HasPrecision(18, 2);
        builder.Property(step => step.ActualCost).HasPrecision(18, 2);
        builder.Property(step => step.Pressure).HasPrecision(10, 3);
        builder.Property(step => step.Humidity).HasPrecision(5, 2);
        builder.Property(step => step.PlannedOutputQuantity).HasPrecision(18, 3);
        builder.Property(step => step.ActualOutputQuantity).HasPrecision(18, 3);
        builder.Property(step => step.WasteQuantity).HasPrecision(18, 3);
        builder.Property(step => step.EnergyConsumption).HasPrecision(18, 3);
        builder.Property(step => step.Instructions).HasMaxLength(4000);
        builder.Property(step => step.ValidationCriteria).HasMaxLength(2000);
        builder.Property(step => step.Observations).HasMaxLength(4000);
        builder.Property(step => step.Status).HasConversion<int>();

        builder.HasOne(step => step.ProductVersion)
            .WithMany(version => version.ProductionSteps)
            .HasForeignKey(step => new { step.ProductId, step.ProductVersionNumber })
            .HasPrincipalKey(version => new { version.ProductId, version.VersionNumber })
            .OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
