using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Catalog;
using ProductApp.Domain.Common;
using ProductApp.Domain.Resources;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.Name).IsUnique(); builder.ConfigureAudit();
        builder.HasData(
            Category(SeedIds.BiscuitsCategory, "BISCUITS", "Biscuits", "Produits biscuitiers"),
            Category(SeedIds.ElectronicsCategory, "ELECTRONICS", "Electronique", "Produits electroniques"),
            Category(SeedIds.FoodCategory, "FOOD", "Alimentaire", "Produits alimentaires"),
            Category(SeedIds.BeveragesCategory, "BEVERAGES", "Boissons", "Boissons et liquides"),
            Category(SeedIds.CosmeticsCategory, "COSMETICS", "Cosmetiques", "Produits cosmetiques"),
            Category(SeedIds.TextilesCategory, "TEXTILES", "Textile", "Produits textiles"),
            Category(SeedIds.OtherCategory, "OTHER", "Autres", "Autres produits industriels"));
    }

    private static object Category(Guid id, string code, string name, string description) =>
        new { Id = id, Code = code, Name = name, Description = description, IsActive = true,
            CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate };
}

public sealed class ResourceCategoryConfiguration : IEntityTypeConfiguration<ResourceCategory>
{
    public void Configure(EntityTypeBuilder<ResourceCategory> builder)
    {
        builder.ToTable("ResourceCategories"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ResourceType).HasConversion<int>();
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.ResourceType); builder.ConfigureAudit();
        builder.HasData(
            new { Id = SeedIds.RawMaterialsCategory, Code = "RAW_MATERIAL", Name = "Matières premières", ResourceType = Domain.Common.ResourceType.RawMaterial, Description = "Matières intégrées au produit", IsActive = true, CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate },
            new { Id = SeedIds.EquipmentCategory, Code = "EQUIPMENT", Name = "Équipements", ResourceType = Domain.Common.ResourceType.Equipment, Description = "Machines et équipements industriels", IsActive = true, CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate });
    }
}

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(200); builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30); builder.Property(x => x.Address).HasMaxLength(1000);
        builder.Property(x => x.Country).HasMaxLength(100); builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name); builder.ConfigureAudit();
    }
}

public sealed class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
{
    public void Configure(EntityTypeBuilder<RawMaterial> builder)
    {
        builder.ToTable("RawMaterials", table =>
        {
            table.HasCheckConstraint("CK_RawMaterials_UnitCost", "\"UnitCost\" >= 0");
            table.HasCheckConstraint("CK_RawMaterials_Stock", "\"AvailableStock\" >= 0 AND \"MinimumStock\" >= 0");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.SapCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Designation).HasMaxLength(250).IsRequired(); builder.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        builder.Property(x => x.UnitCost).HasPrecision(18, 4); builder.Property(x => x.AvailableStock).HasPrecision(18, 3);
        builder.Property(x => x.MinimumStock).HasPrecision(18, 3); builder.HasIndex(x => x.SapCode).IsUnique();
        builder.HasIndex(x => new { x.ResourceCategoryId, x.IsActive });
        builder.HasOne(x => x.ResourceCategory).WithMany(x => x.RawMaterials).HasForeignKey(x => x.ResourceCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier).WithMany(x => x.RawMaterials).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);
        builder.ConfigureAudit();
    }
}

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment", table => table.HasCheckConstraint("CK_Equipment_HourlyCost", "\"HourlyCost\" >= 0"));
        builder.HasKey(x => x.Id); builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); builder.Property(x => x.Manufacturer).HasMaxLength(150);
        builder.Property(x => x.Model).HasMaxLength(150); builder.Property(x => x.HourlyCost).HasPrecision(18, 2);
        builder.Property(x => x.AvailabilityStatus).HasConversion<int>(); builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.ResourceCategoryId, x.AvailabilityStatus });
        builder.HasOne(x => x.ResourceCategory).WithMany(x => x.Equipment).HasForeignKey(x => x.ResourceCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
        builder.HasData(
            EquipmentSeed(SeedIds.MixerEquipment, "EQ-MIXER", "Melangeur industriel", 185m),
            EquipmentSeed(SeedIds.OvenEquipment, "EQ-OVEN", "Four industriel", 520m),
            EquipmentSeed(SeedIds.ConveyorEquipment, "EQ-CONVEYOR", "Convoyeur de production", 130m),
            EquipmentSeed(SeedIds.PackagingEquipment, "EQ-PACK", "Machine de conditionnement", 275m),
            EquipmentSeed(SeedIds.AssemblyEquipment, "EQ-ASSEMBLY", "Poste assemblage automatise", 310m),
            EquipmentSeed(SeedIds.TestingEquipment, "EQ-TEST", "Banc de controle qualite", 160m));
    }

    private static object EquipmentSeed(Guid id, string code, string name, decimal cost) =>
        new { Id = id, Code = code, Name = name, HourlyCost = cost,
            AvailabilityStatus = AvailabilityStatus.Available, ResourceCategoryId = SeedIds.EquipmentCategory,
            CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate };
}

public sealed class MachineConfiguration : IEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        builder.ToTable("Machines", table =>
        {
            table.HasCheckConstraint("CK_Machines_Capacity", "\"CapacityPerHour\" > 0");
            table.HasCheckConstraint("CK_Machines_Energy", "\"EnergyConsumptionPerHour\" >= 0");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CapacityPerHour).HasPrecision(18, 3); builder.Property(x => x.CapacityUnit).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EnergyConsumptionPerHour).HasPrecision(18, 3);
        builder.Property(x => x.LastMaintenanceAt).HasColumnType("datetime2");
        builder.Property(x => x.NextMaintenanceAt).HasColumnType("datetime2");
        builder.HasIndex(x => x.EquipmentId).IsUnique(); builder.HasIndex(x => x.SerialNumber).IsUnique();
        builder.HasOne(x => x.Equipment).WithOne(x => x.Machine).HasForeignKey<Machine>(x => x.EquipmentId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class StepResourceConfiguration : IEntityTypeConfiguration<StepResource>
{
    public void Configure(EntityTypeBuilder<StepResource> builder)
    {
        builder.ToTable("StepResources", table =>
        {
            table.HasCheckConstraint("CK_StepResources_Quantities", "\"PlannedQuantity\" >= 0 AND (\"ActualQuantity\" IS NULL OR \"ActualQuantity\" >= 0)");
            table.HasCheckConstraint("CK_StepResources_Costs", "\"UnitCost\" >= 0 AND \"TotalCost\" >= 0");
        });
        builder.HasKey(x => x.Id); builder.Property(x => x.ResourceType).HasConversion<int>();
        builder.Property(x => x.Designation).HasMaxLength(250).IsRequired(); builder.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PlannedQuantity).HasPrecision(18, 3); builder.Property(x => x.ActualQuantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4); builder.Property(x => x.TotalCost).HasPrecision(18, 2);
        builder.Property(x => x.AvailableStock).HasPrecision(18, 3); builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.ExpirationDate).HasColumnType("date"); builder.Property(x => x.AvailabilityStatus).HasConversion<int>();
        builder.HasIndex(x => new { x.ProductionStepId, x.ResourceType }); builder.HasIndex(x => x.RawMaterialId); builder.HasIndex(x => x.EquipmentId);
        builder.HasOne(x => x.ProductionStep).WithMany(x => x.Resources).HasForeignKey(x => x.ProductionStepId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RawMaterial).WithMany(x => x.StepResources).HasForeignKey(x => x.RawMaterialId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Equipment).WithMany(x => x.StepResources).HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
