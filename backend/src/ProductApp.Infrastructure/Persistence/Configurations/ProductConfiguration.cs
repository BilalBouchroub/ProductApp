using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Products;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        const char quote = (char)34;
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_TargetSalePrice", $"{quote}{nameof(Product.TargetSalePrice)}{quote} IS NULL OR {quote}{nameof(Product.TargetSalePrice)}{quote} >= 0");
            table.HasCheckConstraint("CK_Products_BatchQuantity", $"{quote}{nameof(Product.BatchQuantity)}{quote} IS NULL OR {quote}{nameof(Product.BatchQuantity)}{quote} > 0");
        });
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(product => product.Code).IsUnique();

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.SapCode).HasMaxLength(50);
        builder.Property(product => product.ImageUrl).HasMaxLength(2048);
        builder.Property(product => product.ThemeColor).HasMaxLength(7).HasDefaultValue("#2563EB").IsRequired();
        builder.Property(product => product.TargetSalePrice).HasPrecision(18, 2);
        builder.Property(product => product.BatchQuantity).HasPrecision(18, 3);
        builder.Property(product => product.ProductionUnit).HasMaxLength(30);
        builder.Property(product => product.VersionNumber).IsRequired();
        builder.Property(product => product.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(product => product.Status);

        builder.Ignore(product => product.CreatedAtUtc);
        builder.Ignore(product => product.UpdatedAtUtc);
        builder.HasIndex(product => product.SapCode).IsUnique();
        builder.HasIndex(product => product.ProductCategoryId);
        builder.HasOne(product => product.ProductCategory).WithMany(category => category.Products)
            .HasForeignKey(product => product.ProductCategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.ConfigureAudit();
    }
}
