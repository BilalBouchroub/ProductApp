using ProductApp.Domain.Catalog;
using ProductApp.Domain.Common;

namespace ProductApp.Domain.Resources;

public sealed class Equipment : AuditableEntity
{
    private Equipment() { }
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public decimal HourlyCost { get; private set; }
    public AvailabilityStatus AvailabilityStatus { get; private set; }
    public Guid ResourceCategoryId { get; private set; }
    public ResourceCategory ResourceCategory { get; private set; } = null!;
    public Machine? Machine { get; private set; }
    public ICollection<StepResource> StepResources { get; private set; } = new List<StepResource>();
}
