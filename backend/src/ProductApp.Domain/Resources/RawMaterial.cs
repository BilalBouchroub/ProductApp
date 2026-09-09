using ProductApp.Domain.Catalog;
using ProductApp.Domain.Common;

namespace ProductApp.Domain.Resources;

public sealed class RawMaterial : AuditableEntity
{
    private RawMaterial() { }
    public Guid Id { get; private set; }
    public string SapCode { get; private set; } = string.Empty;
    public string Designation { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitCost { get; private set; }
    public decimal AvailableStock { get; private set; }
    public decimal MinimumStock { get; private set; }
    public Guid ResourceCategoryId { get; private set; }
    public ResourceCategory ResourceCategory { get; private set; } = null!;
    public Guid? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<StepResource> StepResources { get; private set; } = new List<StepResource>();
}
