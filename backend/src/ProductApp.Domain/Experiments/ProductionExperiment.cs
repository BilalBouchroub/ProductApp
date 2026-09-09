using ProductApp.Domain.Common;
using ProductApp.Domain.Products;

namespace ProductApp.Domain.Experiments;

public sealed class ProductionExperiment : AuditableEntity
{
    private ProductionExperiment() { }
    public Guid Id { get; private set; }
    public Guid ProductVersionId { get; private set; }
    public ProductVersion ProductVersion { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Objective { get; private set; } = string.Empty;
    public string? Hypothesis { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal PlannedQuantity { get; private set; }
    public decimal? ActualQuantity { get; private set; }
    public decimal PlannedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    public int PlannedDurationMinutes { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    public decimal? WasteRate { get; private set; }
    public ExperimentResult Result { get; private set; }
    public string? Observations { get; private set; }
    public string? Conclusion { get; private set; }
    public ICollection<ExperimentStep> Steps { get; private set; } = new List<ExperimentStep>();

    public static ProductionExperiment Create(Guid productVersionId, string name, string objective,
        string? hypothesis, DateTime startDate, decimal plannedQuantity, decimal plannedCost,
        int plannedDurationMinutes, DateTime? createdAt = null)
    {
        if (productVersionId == Guid.Empty)
            throw new ArgumentException(null, nameof(productVersionId));
        if (plannedQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(plannedQuantity));
        if (plannedCost < 0)
            throw new ArgumentOutOfRangeException(nameof(plannedCost));
        if (plannedDurationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(plannedDurationMinutes));
        var experiment = new ProductionExperiment
        {
            Id = Guid.NewGuid(),
            ProductVersionId = productVersionId,
            Name = name.Trim(),
            Objective = objective.Trim(),
            Hypothesis = hypothesis?.Trim(),
            StartDate = startDate,
            PlannedQuantity = plannedQuantity,
            PlannedCost = plannedCost,
            PlannedDurationMinutes = plannedDurationMinutes,
            Result = ExperimentResult.Cancelled
        };
        experiment.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return experiment;
    }

    public void AddStep(ExperimentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        if (step.ProductionExperimentId != Id) throw new InvalidOperationException("Étape d’expérience incohérente.");
        if (Steps.Any(item => item.Order == step.Order)) throw new InvalidOperationException("L’ordre de l’étape est déjà utilisé.");
        Steps.Add(step);
    }

    public void UpdateNarrative(string objective, string? hypothesis,
        string? observations, string? conclusion)
    {
        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("L'objectif de l'expérience est obligatoire.", nameof(objective));

        Objective = objective.Trim();
        Hypothesis = string.IsNullOrWhiteSpace(hypothesis) ? null : hypothesis.Trim();
        Observations = string.IsNullOrWhiteSpace(observations) ? null : observations.Trim();
        Conclusion = string.IsNullOrWhiteSpace(conclusion) ? null : conclusion.Trim();
        Touch(DateTime.UtcNow);
    }

    public ExperimentSummary Complete(decimal actualQuantity, ExperimentResult result,
        string? observations, string? conclusion, DateTime completedAt)
    {
        if (Steps.Count == 0 || Steps.Any(step => !step.IsValidated))
            throw new InvalidOperationException("Toutes les étapes de l’expérience doivent être renseignées.");
        var summary = ExperimentCalculator.Calculate(Steps, PlannedQuantity, actualQuantity);
        ActualQuantity = actualQuantity;
        ActualCost = summary.ActualCost;
        ActualDurationMinutes = summary.ActualDurationMinutes;
        WasteRate = summary.WasteRate;
        Result = result;
        Observations = observations;
        Conclusion = conclusion;
        EndDate = completedAt;
        Touch(completedAt);
        return summary;
    }
}
