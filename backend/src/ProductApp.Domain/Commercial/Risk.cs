using ProductApp.Domain.Common;

namespace ProductApp.Domain.Commercial;

public sealed class Risk : AuditableEntity
{
    private Risk() { }
    public Guid Id { get; private set; }
    public Guid MarketStudyId { get; private set; }
    public MarketStudy MarketStudy { get; private set; } = null!;
    public string RiskType { get; private set; } = string.Empty;
    public int Probability { get; private set; }
    public int Impact { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? MitigationAction { get; private set; }
    public int Severity => Probability * Impact;

    public static Risk Create(Guid studyId, string riskType, int probability, int impact,
        string description, string? mitigationAction, DateTime? createdAt = null)
    {
        var risk = new Risk { Id = Guid.NewGuid(), MarketStudyId = studyId };
        risk.Apply(riskType, probability, impact, description, mitigationAction);
        risk.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return risk;
    }

    public void Update(string riskType, int probability, int impact,
        string description, string? mitigationAction, DateTime updatedAt)
    {
        Apply(riskType, probability, impact, description, mitigationAction);
        Touch(updatedAt);
    }

    private void Apply(string riskType, int probability, int impact,
        string description, string? mitigationAction)
    {
        if (probability is < 1 or > 5 || impact is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(probability));
        RiskType = riskType.Trim(); Probability = probability; Impact = impact;
        Description = description.Trim(); MitigationAction = mitigationAction?.Trim();
    }
}
