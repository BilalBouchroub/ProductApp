using ProductApp.Domain.Common;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Identity;
using ProductApp.Domain.Products;

namespace ProductApp.Domain.SmartProduct;

public sealed class AiConversation : AuditableEntity
{
    private AiConversation() { }

    private AiConversation(Guid userId, Guid? productId, Guid? experimentId, DateTime createdAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
        Id = Guid.NewGuid();
        UserId = userId;
        ProductId = productId;
        ExperimentId = experimentId;
        Title = "Nouvelle conversation";
        LastMessageAt = createdAt;
        InitializeAudit(createdAt, userId);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public bool IsArchived { get; private set; }
    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid? ExperimentId { get; private set; }
    public ProductionExperiment? Experiment { get; private set; }
    public DateTime LastMessageAt { get; private set; }
    public ICollection<AiMessage> Messages { get; private set; } = new List<AiMessage>();
    public ICollection<AiAttachment> Attachments { get; private set; } = new List<AiAttachment>();

    public static AiConversation Create(Guid userId, Guid? productId, Guid? experimentId, DateTime createdAt) =>
        new(userId, productId, experimentId, createdAt);

    public void Rename(string title, DateTime updatedAt, Guid actorId)
    {
        var normalized = title?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new ArgumentException("Conversation title cannot be empty.", nameof(title));
        Title = normalized.Length > 160 ? normalized[..160] : normalized;
        Touch(updatedAt, actorId);
    }

    public void SetSummary(string? summary, DateTime updatedAt, Guid actorId)
    {
        Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        Touch(updatedAt, actorId);
    }

    public void SetArchived(bool archived, DateTime updatedAt, Guid actorId)
    {
        IsArchived = archived;
        Touch(updatedAt, actorId);
    }

    public void SetContext(Guid? productId, Guid? experimentId, DateTime updatedAt, Guid actorId)
    {
        ProductId = productId;
        ExperimentId = experimentId;
        Touch(updatedAt, actorId);
    }

    public void MarkActivity(DateTime occurredAt, Guid actorId)
    {
        LastMessageAt = occurredAt;
        Touch(occurredAt, actorId);
    }
}
