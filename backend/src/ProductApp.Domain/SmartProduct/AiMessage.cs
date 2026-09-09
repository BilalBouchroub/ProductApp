using ProductApp.Domain.Common;

namespace ProductApp.Domain.SmartProduct;

public sealed class AiMessage : AuditableEntity
{
    private AiMessage() { }

    private AiMessage(Guid conversationId, AiMessageRole role, string content, AiMessageStatus status,
        string? model, DateTime createdAt, Guid actorId)
    {
        if (conversationId == Guid.Empty) throw new ArgumentException("Conversation identifier cannot be empty.", nameof(conversationId));
        if (string.IsNullOrWhiteSpace(content) && role == AiMessageRole.User)
            throw new ArgumentException("User message cannot be empty.", nameof(content));
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Role = role;
        Content = content?.Trim() ?? string.Empty;
        Status = status;
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        InitializeAudit(createdAt, actorId);
    }

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public AiConversation Conversation { get; private set; } = null!;
    public AiMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public AiMessageStatus Status { get; private set; }
    public string? Model { get; private set; }
    public int? InputTokens { get; private set; }
    public int? OutputTokens { get; private set; }
    public long? DurationMilliseconds { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? StructuredContentJson { get; private set; }
    public ICollection<AiMessageSource> Sources { get; private set; } = new List<AiMessageSource>();
    public ICollection<AiFeedback> Feedback { get; private set; } = new List<AiFeedback>();

    public static AiMessage CreateUser(Guid conversationId, string content, DateTime createdAt, Guid userId) =>
        new(conversationId, AiMessageRole.User, content, AiMessageStatus.Completed, null, createdAt, userId);

    public static AiMessage CreateAssistant(Guid conversationId, string model, DateTime createdAt, Guid userId) =>
        new(conversationId, AiMessageRole.Assistant, string.Empty, AiMessageStatus.Pending, model, createdAt, userId);

    public void Start(DateTime updatedAt, Guid actorId)
    {
        Status = AiMessageStatus.Streaming;
        ErrorCode = null;
        Touch(updatedAt, actorId);
    }

    public void Complete(string content, string? structuredContentJson, int? inputTokens, int? outputTokens,
        long durationMilliseconds, DateTime updatedAt, Guid actorId)
    {
        Content = content?.Trim() ?? string.Empty;
        StructuredContentJson = string.IsNullOrWhiteSpace(structuredContentJson) ? null : structuredContentJson;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        Status = AiMessageStatus.Completed;
        ErrorCode = null;
        Touch(updatedAt, actorId);
    }

    public void Cancel(string partialContent, long durationMilliseconds, DateTime updatedAt, Guid actorId)
    {
        Content = partialContent?.Trim() ?? string.Empty;
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        Status = AiMessageStatus.Cancelled;
        Touch(updatedAt, actorId);
    }

    public void Fail(string safeErrorCode, long durationMilliseconds, DateTime updatedAt, Guid actorId)
    {
        ErrorCode = string.IsNullOrWhiteSpace(safeErrorCode) ? "ai_generation_failed" : safeErrorCode.Trim();
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        Status = AiMessageStatus.Failed;
        Touch(updatedAt, actorId);
    }
}

public sealed class AiMessageSource : AuditableEntity
{
    private AiMessageSource() { }
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public AiMessage Message { get; private set; } = null!;
    public AiSourceKind Kind { get; private set; }
    public Guid? EntityId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Reference { get; private set; }
    public string? InternalUrl { get; private set; }
    public string? Excerpt { get; private set; }

    public static AiMessageSource Create(Guid messageId, AiSourceKind kind, Guid? entityId, string label,
        string? reference, string? internalUrl, string? excerpt, DateTime createdAt, Guid actorId)
    {
        if (messageId == Guid.Empty) throw new ArgumentException("Message identifier cannot be empty.", nameof(messageId));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Source label cannot be empty.", nameof(label));
        var source = new AiMessageSource
        {
            Id = Guid.NewGuid(), MessageId = messageId, Kind = kind, EntityId = entityId,
            Label = label.Trim(), Reference = Normalize(reference), InternalUrl = Normalize(internalUrl),
            Excerpt = Normalize(excerpt)
        };
        source.InitializeAudit(createdAt, actorId);
        return source;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class AiFeedback : AuditableEntity
{
    private AiFeedback() { }
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public AiMessage Message { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public AiFeedbackRating Rating { get; private set; }
    public string? Comment { get; private set; }

    public static AiFeedback Create(Guid messageId, Guid userId, AiFeedbackRating rating, string? comment, DateTime createdAt)
    {
        if (messageId == Guid.Empty || userId == Guid.Empty) throw new ArgumentException("Identifiers cannot be empty.");
        var feedback = new AiFeedback
        {
            Id = Guid.NewGuid(), MessageId = messageId, UserId = userId, Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
        };
        feedback.InitializeAudit(createdAt, userId);
        return feedback;
    }
}
