using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class Notification : AuditableEntity
{
    private Notification() { }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    public static Notification Create(Guid userId, string title, string message,
        NotificationType type, string? relatedEntityType, Guid? relatedEntityId,
        DateTime utcNow, Guid? actorId = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.", nameof(message));
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };
        notification.InitializeAudit(utcNow, actorId);
        return notification;
    }

    public void MarkRead(DateTime utcNow, Guid userId) { if (IsRead) return; IsRead = true; ReadAt = utcNow; Touch(utcNow, userId); }
}
