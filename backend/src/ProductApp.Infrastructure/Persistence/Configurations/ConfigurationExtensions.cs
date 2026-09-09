using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Common;
using ProductApp.Domain.Identity;

namespace ProductApp.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static void ConfigureAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(entity => entity.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(entity => entity.CreatedAt);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(entity => entity.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(entity => entity.UpdatedBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

internal static class SeedIds
{
    public static readonly Guid AdministratorRole = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid ProductionRole = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid CommercialRole = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid AdministratorUser = Guid.Parse("50000000-0000-0000-0000-000000000001");
    public static readonly Guid ProductionUser = Guid.Parse("50000000-0000-0000-0000-000000000002");
    public static readonly Guid CommercialUser = Guid.Parse("50000000-0000-0000-0000-000000000003");
    public static readonly Guid BiscuitsCategory = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid ElectronicsCategory = Guid.Parse("30000000-0000-0000-0000-000000000002");
    public static readonly Guid FoodCategory = Guid.Parse("30000000-0000-0000-0000-000000000003");
    public static readonly Guid BeveragesCategory = Guid.Parse("30000000-0000-0000-0000-000000000004");
    public static readonly Guid CosmeticsCategory = Guid.Parse("30000000-0000-0000-0000-000000000005");
    public static readonly Guid TextilesCategory = Guid.Parse("30000000-0000-0000-0000-000000000006");
    public static readonly Guid OtherCategory = Guid.Parse("30000000-0000-0000-0000-000000000007");
    public static readonly Guid MixerEquipment = Guid.Parse("60000000-0000-0000-0000-000000000001");
    public static readonly Guid OvenEquipment = Guid.Parse("60000000-0000-0000-0000-000000000002");
    public static readonly Guid ConveyorEquipment = Guid.Parse("60000000-0000-0000-0000-000000000003");
    public static readonly Guid PackagingEquipment = Guid.Parse("60000000-0000-0000-0000-000000000004");
    public static readonly Guid AssemblyEquipment = Guid.Parse("60000000-0000-0000-0000-000000000005");
    public static readonly Guid TestingEquipment = Guid.Parse("60000000-0000-0000-0000-000000000006");
    public static readonly Guid RawMaterialsCategory = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid EquipmentCategory = Guid.Parse("40000000-0000-0000-0000-000000000002");
    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
