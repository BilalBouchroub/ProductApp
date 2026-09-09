using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.Identity;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers", table => table.HasCheckConstraint("CK_ApplicationUsers_Email", "\"NormalizedEmail\" <> ''"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(100).IsConcurrencyToken().IsRequired();
        builder.Property(x => x.LockoutEnd).HasColumnType("datetimeoffset");
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.LastLoginAt).HasColumnType("datetime2");
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasIndex(x => x.NormalizedUserName).IsUnique();
        builder.HasIndex(x => new { x.RoleId, x.Status });
        builder.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
        builder.HasData(
            DemoUser(SeedIds.AdministratorUser, "Admin", "ProductApp", "admin@productapp.local", SeedIds.AdministratorRole, "AQAAAAIAAYagAAAAEJGBIicEG6k57BLipKy6/y2Eg7ApXjqDTF4hhxmZCCQVu2zxKbb+NVK0DthqUvGOWg==", "admin-security", "admin-concurrency"),
            DemoUser(SeedIds.ProductionUser, "Responsable", "Production", "production@productapp.local", SeedIds.ProductionRole, "AQAAAAIAAYagAAAAELO6DeEckMvLmIJh6FpsWsMdyaEirSCzloiWaM7Ia3gTNAcSFYYqAk5Wv1cu9O/4mA==", "production-security", "production-concurrency"),
            DemoUser(SeedIds.CommercialUser, "Responsable", "Commercial", "commercial@productapp.local", SeedIds.CommercialRole, "AQAAAAIAAYagAAAAEAyALAU+9GPoV+klxaFQm8O/tLB8qmQkgPH4TqdBRpgRkgsCzqN6wlmcppz/skZo4w==", "commercial-security", "commercial-concurrency"));
    }

    private static object DemoUser(Guid id, string firstName, string lastName, string email, Guid roleId, string passwordHash, string securityStamp, string concurrencyStamp) => new
    {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        PhoneNumber = (string?)null,
        PhoneNumberConfirmed = false,
        PasswordHash = passwordHash,
        SecurityStamp = securityStamp,
        ConcurrencyStamp = concurrencyStamp,
        TwoFactorEnabled = false,
        LockoutEnd = (DateTimeOffset?)null,
        LockoutEnabled = true,
        AccessFailedCount = 0,
        MustChangePassword = true,
        Status = ProductApp.Domain.Common.UserStatus.Active,
        AvatarUrl = (string?)null,
        LastLoginAt = (DateTime?)null,
        RoleId = (Guid?)roleId,
        CreatedAt = SeedIds.SeedDate,
        UpdatedAt = SeedIds.SeedDate,
        CreatedBy = (Guid?)null,
        UpdatedBy = (Guid?)null
    };
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(100).IsConcurrencyToken().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique(); builder.HasIndex(x => x.NormalizedName).IsUnique(); builder.ConfigureAudit();
        builder.HasData(
            new { Id = SeedIds.AdministratorRole, Name = "Administrator", NormalizedName = "ADMINISTRATOR", ConcurrencyStamp = "role-admin", Description = "Administration complète de la plateforme", CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate },
            new { Id = SeedIds.ProductionRole, Name = "ProductionManager", NormalizedName = "PRODUCTIONMANAGER", ConcurrencyStamp = "role-production", Description = "Gestion des produits et de la production", CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate },
            new { Id = SeedIds.CommercialRole, Name = "CommercialManager", NormalizedName = "COMMERCIALMANAGER", ConcurrencyStamp = "role-commercial", Description = "Études commerciales et décisions de marché", CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate });
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    private static readonly (Guid Id, string Code, string Name, string Module)[] Values =
    [
        (Guid.Parse("20000000-0000-0000-0000-000000000001"), "users.manage", "Gérer les utilisateurs", "Users"),
        (Guid.Parse("20000000-0000-0000-0000-000000000002"), "audit.read", "Consulter les audits", "Audit"),
        (Guid.Parse("20000000-0000-0000-0000-000000000003"), "products.manage", "Gérer les produits", "Products"),
        (Guid.Parse("20000000-0000-0000-0000-000000000004"), "experiments.manage", "Gérer les expériences", "Experiments"),
        (Guid.Parse("20000000-0000-0000-0000-000000000005"), "studies.manage", "Gérer les études de marché", "MarketStudies")
    ];

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.Module); builder.ConfigureAudit();
        builder.HasData(Values.Select(x => new { x.Id, x.Code, x.Name, x.Module, Description = x.Name, CreatedAt = SeedIds.SeedDate, UpdatedAt = SeedIds.SeedDate }));
    }

    internal static IEnumerable<Guid> AllIds => Values.Select(x => x.Id);
    internal static IEnumerable<Guid> ProductionIds => Values.Where(x => x.Module is "Products" or "Experiments").Select(x => x.Id);
    internal static IEnumerable<Guid> CommercialIds => Values.Where(x => x.Module == "MarketStudies").Select(x => x.Id);
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions"); builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        var rows = PermissionConfiguration.AllIds.Select(id => new { RoleId = SeedIds.AdministratorRole, PermissionId = id })
            .Concat(PermissionConfiguration.ProductionIds.Select(id => new { RoleId = SeedIds.ProductionRole, PermissionId = id }))
            .Concat(PermissionConfiguration.CommercialIds.Select(id => new { RoleId = SeedIds.CommercialRole, PermissionId = id }));
        builder.HasData(rows);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens"); builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("datetime2");
        builder.Property(x => x.RevokedAt).HasColumnType("datetime2");
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);
        builder.Property(x => x.CreatedByIp).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RevokedByIp).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt });
        builder.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.ReadAt).HasColumnType("datetime2");
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Timestamp).HasColumnType("datetime2");
        builder.Property(x => x.Action).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Level).HasConversion<int>();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.ChangesJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => x.Timestamp); builder.HasIndex(x => new { x.Module, x.Level });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.ConfigureAudit();
    }
}
