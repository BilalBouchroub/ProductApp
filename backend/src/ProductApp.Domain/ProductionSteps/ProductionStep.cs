using ProductApp.Domain.Common;
using ProductApp.Domain.Products;
using ProductApp.Domain.Resources;

namespace ProductApp.Domain.ProductionSteps;

public sealed class ProductionStep : AuditableEntity
{
    private ProductionStep()
    {
    }

    private ProductionStep(
        Guid id,
        Guid productId,
        int order,
        string name,
        string? description,
        int durationMinutes,
        decimal temperature,
        string equipmentName,
        decimal laborCost)
    {
        Id = id;
        ProductId = productId;
        ProductVersionNumber = 1;
        InitializeAudit(DateTime.UtcNow);
        ApplyValues(order, name, description, durationMinutes, temperature, equipmentName, laborCost);
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public int ProductVersionNumber { get; private set; }
    public ProductVersion ProductVersion { get; private set; } = null!;

    public int Order { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Icon { get; private set; } = ProductionStepIconNames.Automatic;

    public string? Description { get; private set; }

    public int DurationMinutes { get; private set; }

    public decimal Temperature { get; private set; }

    public string EquipmentName { get; private set; } = string.Empty;

    public decimal LaborCost { get; private set; }
    public decimal? PlannedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    public decimal? Pressure { get; private set; }
    public decimal? Humidity { get; private set; }
    public decimal? PlannedOutputQuantity { get; private set; }
    public decimal? ActualOutputQuantity { get; private set; }
    public decimal? WasteQuantity { get; private set; }
    public int OperatorCount { get; private set; }
    public decimal EnergyConsumption { get; private set; }
    public string? Instructions { get; private set; }
    public string? ValidationCriteria { get; private set; }
    public string? Observations { get; private set; }
    public RecordStatus Status { get; private set; } = RecordStatus.Active;
    public ICollection<StepResource> Resources { get; private set; } = new List<StepResource>();
    public bool IsValid => Order > 0 && !string.IsNullOrWhiteSpace(Name) && DurationMinutes > 0
        && LaborCost >= 0 && Status is RecordStatus.Active or RecordStatus.Validated;

    public static ProductionStep Create(
        Guid productId,
        int order,
        string name,
        string? description,
        int durationMinutes,
        decimal temperature,
        string equipmentName,
        decimal laborCost)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product identifier cannot be empty.", nameof(productId));
        }

        return new ProductionStep(
            Guid.NewGuid(), productId, order, name, description,
            durationMinutes, temperature, equipmentName, laborCost);
    }

    public static ProductionStep CreateForVersion(
        Guid productId, int productVersionNumber, int order, string name,
        string? description, int durationMinutes, decimal temperature,
        string equipmentName, decimal laborCost)
    {
        var step = Create(productId, order, name, description, durationMinutes,
            temperature, equipmentName, laborCost);
        step.ProductVersionNumber = productVersionNumber;
        return step;
    }

    public void AddResource(StepResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (resource.ProductionStepId != Id)
        {
            throw new InvalidOperationException("La ressource doit appartenir à cette étape.");
        }

        Resources.Add(resource);
        Touch(DateTime.UtcNow);
    }

    public void Update(
        int order,
        string name,
        string? description,
        int durationMinutes,
        decimal temperature,
        string equipmentName,
        decimal laborCost)
    {
        ApplyValues(order, name, description, durationMinutes, temperature, equipmentName, laborCost);
    }

    public void ChangeOrder(int order)
    {
        if (order < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be greater than zero.");
        }

        Order = order;
    }

    public void SetIcon(string? icon)
    {
        Icon = string.IsNullOrWhiteSpace(icon) ? ProductionStepIconNames.Automatic : icon.Trim();
        Touch(DateTime.UtcNow);
    }

    public void ConfigureDetails(
        int? actualDurationMinutes,
        decimal? plannedCost,
        decimal? actualCost,
        decimal? pressure,
        decimal? humidity,
        decimal? plannedOutputQuantity,
        decimal? actualOutputQuantity,
        decimal? wasteQuantity,
        int operatorCount,
        decimal energyConsumption,
        string? instructions,
        string? validationCriteria,
        string? observations,
        RecordStatus status)
    {
        if (actualDurationMinutes < 0 || plannedCost < 0 || actualCost < 0
            || plannedOutputQuantity < 0 || actualOutputQuantity < 0 || wasteQuantity < 0
            || operatorCount < 0 || energyConsumption < 0)
            throw new ArgumentOutOfRangeException(nameof(plannedCost), "Les valeurs de production ne peuvent pas être négatives.");
        if (humidity is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(humidity));

        ActualDurationMinutes = actualDurationMinutes;
        PlannedCost = plannedCost;
        ActualCost = actualCost;
        Pressure = pressure;
        Humidity = humidity;
        PlannedOutputQuantity = plannedOutputQuantity;
        ActualOutputQuantity = actualOutputQuantity;
        WasteQuantity = wasteQuantity;
        OperatorCount = operatorCount;
        EnergyConsumption = energyConsumption;
        Instructions = NormalizeOptional(instructions);
        ValidationCriteria = NormalizeOptional(validationCriteria);
        Observations = NormalizeOptional(observations);
        Status = status;
        Touch(DateTime.UtcNow);
    }

    private void ApplyValues(
        int order,
        string name,
        string? description,
        int durationMinutes,
        decimal temperature,
        string equipmentName,
        decimal laborCost)
    {
        ChangeOrder(order);
        Name = NormalizeRequired(name, nameof(name));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (durationMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        }

        if (temperature < -273.15m)
        {
            throw new ArgumentOutOfRangeException(nameof(temperature));
        }

        if (laborCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(laborCost));
        }

        DurationMinutes = durationMinutes;
        Temperature = temperature;
        EquipmentName = NormalizeRequired(equipmentName, nameof(equipmentName));
        LaborCost = laborCost;
        Touch(DateTime.UtcNow);
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
