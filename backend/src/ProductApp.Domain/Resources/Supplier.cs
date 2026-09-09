using ProductApp.Domain.Common;

namespace ProductApp.Domain.Resources;

public sealed class Supplier : AuditableEntity
{
    private Supplier() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string? Country { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<RawMaterial> RawMaterials { get; private set; } = new List<RawMaterial>();
}
