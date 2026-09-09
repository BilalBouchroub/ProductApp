using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class Role : AuditableEntity
{
    private Role() { }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string ConcurrencyStamp { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ICollection<ApplicationUser> Users { get; private set; } = new List<ApplicationUser>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public static Role Create(string name, string description)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        };
        role.InitializeAudit(DateTime.UtcNow);
        return role;
    }

    public void SetName(string? value) { Name = value?.Trim() ?? string.Empty; Touch(DateTime.UtcNow); }
    public void SetNormalizedName(string? value) { NormalizedName = value ?? string.Empty; }
    public void SetConcurrencyStamp(string? value) { ConcurrencyStamp = value ?? Guid.NewGuid().ToString("N"); }
}

public sealed class RolePermission
{
    private RolePermission() { }
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;
}
