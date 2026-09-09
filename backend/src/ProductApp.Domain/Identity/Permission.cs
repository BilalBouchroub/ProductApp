using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class Permission : AuditableEntity
{
    private Permission() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
}
