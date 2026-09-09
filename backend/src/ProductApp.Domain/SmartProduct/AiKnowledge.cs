using ProductApp.Domain.Common;

namespace ProductApp.Domain.SmartProduct;

public sealed class AiAttachment : AuditableEntity
{
    private AiAttachment() { }
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public AiConversation Conversation { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid? MessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;
    public AiAttachmentStatus Status { get; private set; }
    public string? ErrorCode { get; private set; }

    public static AiAttachment Create(Guid conversationId, Guid userId, string fileName, string storedFileName,
        string storageKey, string mimeType, long fileSize, string sha256, DateTime createdAt)
    {
        if (conversationId == Guid.Empty || userId == Guid.Empty) throw new ArgumentException("Identifiers cannot be empty.");
        if (fileSize <= 0) throw new ArgumentOutOfRangeException(nameof(fileSize));
        var attachment = new AiAttachment
        {
            Id = Guid.NewGuid(), ConversationId = conversationId, UserId = userId,
            FileName = fileName.Trim(), StoredFileName = storedFileName.Trim(), StorageKey = storageKey.Trim(),
            MimeType = mimeType.Trim().ToLowerInvariant(), FileSize = fileSize, Sha256 = sha256,
            Status = AiAttachmentStatus.Uploaded
        };
        attachment.InitializeAudit(createdAt, userId);
        return attachment;
    }

    public void AttachToMessage(Guid messageId, DateTime updatedAt)
    {
        MessageId = messageId;
        Touch(updatedAt, UserId);
    }

    public void BeginIndexing(DateTime updatedAt) { Status = AiAttachmentStatus.Indexing; ErrorCode = null; Touch(updatedAt, UserId); }
    public void MarkIndexed(DateTime updatedAt) { Status = AiAttachmentStatus.Indexed; ErrorCode = null; Touch(updatedAt, UserId); }
    public void MarkFailed(string errorCode, DateTime updatedAt) { Status = AiAttachmentStatus.Failed; ErrorCode = errorCode; Touch(updatedAt, UserId); }
}

public sealed class AiDocumentChunk : AuditableEntity
{
    private AiDocumentChunk() { }
    public Guid Id { get; private set; }
    public Guid AttachmentId { get; private set; }
    public AiAttachment Attachment { get; private set; } = null!;
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? ExperimentId { get; private set; }
    public int? PageNumber { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? EmbeddingJson { get; private set; }

    public static AiDocumentChunk Create(Guid attachmentId, Guid conversationId, Guid userId,
        Guid? productId, Guid? experimentId, int? pageNumber, int chunkIndex, string source,
        string content, string? embeddingJson, DateTime createdAt)
    {
        if (attachmentId == Guid.Empty || conversationId == Guid.Empty || userId == Guid.Empty)
            throw new ArgumentException("Identifiers cannot be empty.");
        if (chunkIndex < 0) throw new ArgumentOutOfRangeException(nameof(chunkIndex));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Chunk content cannot be empty.", nameof(content));
        var chunk = new AiDocumentChunk
        {
            Id = Guid.NewGuid(), AttachmentId = attachmentId, ConversationId = conversationId, UserId = userId,
            ProductId = productId, ExperimentId = experimentId, PageNumber = pageNumber, ChunkIndex = chunkIndex,
            Source = source.Trim(), Content = content.Trim(), EmbeddingJson = embeddingJson
        };
        chunk.InitializeAudit(createdAt, userId);
        return chunk;
    }
}

public sealed class AiToolInvocation : AuditableEntity
{
    private AiToolInvocation() { }
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid UserId { get; private set; }
    public string ToolName { get; private set; } = string.Empty;
    public AiToolInvocationStatus Status { get; private set; }
    public long DurationMilliseconds { get; private set; }
    public string? SafeErrorCode { get; private set; }
    public string? ArgumentsSummary { get; private set; }

    public static AiToolInvocation Create(Guid conversationId, Guid messageId, Guid userId,
        string toolName, string? argumentsSummary, DateTime createdAt)
    {
        var invocation = new AiToolInvocation
        {
            Id = Guid.NewGuid(), ConversationId = conversationId, MessageId = messageId, UserId = userId,
            ToolName = toolName.Trim(), ArgumentsSummary = string.IsNullOrWhiteSpace(argumentsSummary) ? null : argumentsSummary,
            Status = AiToolInvocationStatus.Started
        };
        invocation.InitializeAudit(createdAt, userId);
        return invocation;
    }

    public void Finish(AiToolInvocationStatus status, long durationMilliseconds, string? safeErrorCode, DateTime updatedAt)
    {
        Status = status;
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        SafeErrorCode = string.IsNullOrWhiteSpace(safeErrorCode) ? null : safeErrorCode;
        Touch(updatedAt, UserId);
    }
}
