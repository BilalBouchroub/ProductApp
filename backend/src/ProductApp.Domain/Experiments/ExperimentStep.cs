using ProductApp.Domain.Common;
using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Domain.Experiments;

public sealed class ExperimentStep : AuditableEntity
{
    private ExperimentStep() { }
    public Guid Id { get; private set; }
    public Guid ProductionExperimentId { get; private set; }
    public ProductionExperiment ProductionExperiment { get; private set; } = null!;
    public Guid ProductionStepId { get; private set; }
    public ProductionStep ProductionStep { get; private set; } = null!;
    public int Order { get; private set; }
    public decimal PlannedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    public int PlannedDurationMinutes { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    public decimal? PlannedOutputQuantity { get; private set; }
    public decimal? ActualOutputQuantity { get; private set; }
    public string? Observations { get; private set; }
    public bool IsValidated { get; private set; }

    public static ExperimentStep Create(Guid experimentId, Guid productionStepId, int order,
        decimal plannedCost, int plannedDurationMinutes, decimal? plannedOutputQuantity,
        DateTime? createdAt = null)
    {
        if (order < 1 || plannedCost < 0 || plannedDurationMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(order));
        var step = new ExperimentStep
        {
            Id = Guid.NewGuid(),
            ProductionExperimentId = experimentId,
            ProductionStepId = productionStepId,
            Order = order,
            PlannedCost = plannedCost,
            PlannedDurationMinutes = plannedDurationMinutes,
            PlannedOutputQuantity = plannedOutputQuantity
        };
        step.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return step;
    }

    public void RecordActuals(decimal actualCost, int actualDurationMinutes,
        decimal? actualOutputQuantity, string? observations, DateTime updatedAt)
    {
        if (actualCost < 0 || actualDurationMinutes < 0 || actualOutputQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(actualCost));
        ActualCost = actualCost;
        ActualDurationMinutes = actualDurationMinutes;
        ActualOutputQuantity = actualOutputQuantity;
        Observations = observations;
        IsValidated = true;
        Touch(updatedAt);
    }
}
