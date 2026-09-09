using ProductApp.Domain.Common;

namespace ProductApp.Domain.Resources;

public sealed class Machine : AuditableEntity
{
    private Machine() { }
    public Guid Id { get; private set; }
    public Guid EquipmentId { get; private set; }
    public Equipment Equipment { get; private set; } = null!;
    public string SerialNumber { get; private set; } = string.Empty;
    public decimal CapacityPerHour { get; private set; }
    public string CapacityUnit { get; private set; } = string.Empty;
    public decimal EnergyConsumptionPerHour { get; private set; }
    public DateTime? LastMaintenanceAt { get; private set; }
    public DateTime? NextMaintenanceAt { get; private set; }
}
