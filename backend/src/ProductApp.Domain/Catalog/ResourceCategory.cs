using ProductApp.Domain.Common;

namespace ProductApp.Domain.Catalog;

public sealed class ResourceCategory : AuditableEntity
{
    private ResourceCategory() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ResourceType ResourceType { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Resources.RawMaterial> RawMaterials { get; private set; } = new List<Resources.RawMaterial>();
    public ICollection<Resources.Equipment> Equipment { get; private set; } = new List<Resources.Equipment>();
}
