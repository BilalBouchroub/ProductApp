using ProductApp.Domain.Common;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Resources;

public sealed class StepResource : AuditableEntity
{
    private StepResource() { }
    public Guid Id { get; private set; }
    public Guid ProductionStepId { get; private set; }
    public ProductionStep ProductionStep { get; private set; } = null!;
    public ResourceType ResourceType { get; private set; }
    public Guid? RawMaterialId { get; private set; }
    public RawMaterial? RawMaterial { get; private set; }
    public Guid? EquipmentId { get; private set; }
    public Equipment? Equipment { get; private set; }
    public string Designation { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal PlannedQuantity { get; private set; }
    public decimal? ActualQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal? AvailableStock { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public AvailabilityStatus AvailabilityStatus { get; private set; }

    public static StepResource Create(Guid stepId, ResourceType resourceType, string designation,
        string unit, decimal plannedQuantity, decimal? actualQuantity, decimal unitCost,
        decimal? availableStock, Guid? rawMaterialId = null, Guid? equipmentId = null,
        DateTime? createdAt = null)
    {
        var result = ResourceCalculator.Calculate(plannedQuantity, actualQuantity, unitCost, availableStock);
        var resource = new StepResource
        {
            Id = Guid.NewGuid(),
            ProductionStepId = stepId,
            ResourceType = resourceType,
            Designation = designation.Trim(),
            Unit = unit.Trim(),
            PlannedQuantity = plannedQuantity,
            ActualQuantity = actualQuantity,
            UnitCost = unitCost,
            TotalCost = result.PlannedCost,
            AvailableStock = availableStock,
            AvailabilityStatus = result.Availability,
            RawMaterialId = rawMaterialId,
            EquipmentId = equipmentId
        };
        resource.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return resource;
    }

    public void UpdateQuantities(decimal plannedQuantity, decimal? actualQuantity,
        decimal unitCost, decimal? availableStock, DateTime updatedAt)
    {
        var result = ResourceCalculator.Calculate(plannedQuantity, actualQuantity, unitCost, availableStock);
        PlannedQuantity = plannedQuantity;
        ActualQuantity = actualQuantity;
        UnitCost = unitCost;
        TotalCost = result.PlannedCost;
        AvailableStock = availableStock;
        AvailabilityStatus = result.Availability;
        Touch(updatedAt);
    }
}
