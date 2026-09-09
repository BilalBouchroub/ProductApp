using ProductApp.Domain.SmartProduct;

namespace ProductApp.Application.SmartProduct;

public sealed record AiUserContext(Guid UserId, string Role, string DisplayName);
public sealed record CreateAiConversationRequest(Guid? ProductId = null, Guid? ExperimentId = null);
public sealed record UpdateAiConversationRequest(string? Title = null, bool? IsArchived = null);
public sealed record AiConversationListItemDto(Guid Id, string Title, bool IsArchived, Guid? ProductId,
    Guid? ExperimentId, DateTime CreatedAt, DateTime UpdatedAt, DateTime LastMessageAt, string? Preview);
public sealed record AiConversationDto(Guid Id, string Title, bool IsArchived, Guid? ProductId,
    Guid? ExperimentId, DateTime CreatedAt, DateTime UpdatedAt, IReadOnlyList<AiMessageDto> Messages,
    IReadOnlyList<AiAttachmentDto> Attachments);
public sealed record AiMessageDto(Guid Id, AiMessageRole Role, string Content, AiMessageStatus Status,
    string? Model, int? InputTokens, int? OutputTokens, long? DurationMilliseconds, string? ErrorCode,
    string? StructuredContentJson, DateTime CreatedAt, IReadOnlyList<AiSourceDto> Sources);
public sealed record AiSourceDto(Guid Id, AiSourceKind Kind, Guid? EntityId, string Label,
    string? Reference, string? InternalUrl, string? Excerpt);
public sealed record AiAttachmentDto(Guid Id, string FileName, string MimeType, long FileSize,
    AiAttachmentStatus Status, string? ErrorCode, DateTime CreatedAt);
public sealed record SendAiMessageRequest(string Content, IReadOnlyList<Guid>? AttachmentIds = null);
public sealed record AiFeedbackRequest(AiFeedbackRating Rating, string? Comment = null);
public sealed record AiStreamEvent(string Type, object? Data);
public sealed record AiProviderMessage(string Role, string Content);
public sealed record AiProviderRequest(string Instructions, IReadOnlyList<AiProviderMessage> Messages,
    int MaxOutputTokens, double Temperature = 0.2);
public sealed record AiProviderDelta(string? Text, bool IsCompleted, int? InputTokens = null,
    int? OutputTokens = null, string? ErrorCode = null);
public sealed record AiToolSource(AiSourceKind Kind, Guid? EntityId, string Label,
    string? Reference = null, string? InternalUrl = null, string? Excerpt = null);
public sealed record AiToolResult(string Name, string Json, IReadOnlyList<AiToolSource> Sources);
public sealed record AiBusinessContext(IReadOnlyList<AiToolResult> Results);
public sealed record StoredAiFile(string StoredFileName, string StorageKey, string Sha256);
public sealed record ExtractedDocumentPage(int? PageNumber, string Text);
public sealed record AiDocumentSearchResult(Guid AttachmentId, string FileName, int? PageNumber,
    int ChunkIndex, string Content, double Score);

public sealed class AiNotFoundException(string resource) : Exception($"{resource} est introuvable.");
public sealed class AiAccessDeniedException() : Exception("Vous n’avez pas accès à cette ressource SMART PRODUCT.");
public sealed class AiProviderUnavailableException(string safeCode, Exception? inner = null)
    : Exception("Le service SMART PRODUCT est temporairement indisponible.", inner)
{
    public string SafeCode { get; } = safeCode;
}
public sealed class AiAttachmentValidationException(string message) : Exception(message);

public interface IAiConversationRepository
{
    Task<AiConversation?> GetOwnedAsync(Guid conversationId, Guid userId, bool includeDetails, CancellationToken ct);
    Task<IReadOnlyList<AiConversation>> SearchOwnedAsync(Guid userId, string? query, bool includeArchived, CancellationToken ct);
    Task<AiMessage?> GetOwnedMessageAsync(Guid messageId, Guid userId, CancellationToken ct);
    Task<AiAttachment?> GetOwnedAttachmentAsync(Guid attachmentId, Guid userId, CancellationToken ct);
    Task AddConversationAsync(AiConversation conversation, CancellationToken ct);
    Task AddMessageAsync(AiMessage message, CancellationToken ct);
    Task AddAttachmentAsync(AiAttachment attachment, CancellationToken ct);
    Task AddSourcesAsync(IEnumerable<AiMessageSource> sources, CancellationToken ct);
    Task AddFeedbackAsync(AiFeedback feedback, CancellationToken ct);
    Task AddToolInvocationAsync(AiToolInvocation invocation, CancellationToken ct);
    Task ReplaceChunksAsync(Guid attachmentId, IEnumerable<AiDocumentChunk> chunks, CancellationToken ct);
    Task<IReadOnlyList<AiDocumentChunk>> GetAuthorizedChunksAsync(Guid userId, Guid conversationId, CancellationToken ct);
    void RemoveConversation(AiConversation conversation);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IAiProvider
{
    string Model { get; }
    IAsyncEnumerable<AiProviderDelta> StreamAsync(AiProviderRequest request, CancellationToken ct);
    Task<IReadOnlyList<float>?> CreateEmbeddingAsync(string text, CancellationToken ct);
}

public interface IAiBusinessToolCatalog
{
    Task<AiBusinessContext> BuildAuthorizedContextAsync(AiUserContext user, Guid? productId,
        Guid? experimentId, string prompt, CancellationToken ct);
}

public interface IAiAttachmentStorage
{
    Task<StoredAiFile> SaveAsync(Guid userId, Stream content, string safeExtension, CancellationToken ct);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
    Task DeleteAsync(string storageKey, CancellationToken ct);
}

public interface IAiDocumentExtractor
{
    bool Supports(string extension);
    Task<IReadOnlyList<ExtractedDocumentPage>> ExtractAsync(Stream content, string extension, CancellationToken ct);
}

public interface IAiKnowledgeService
{
    Task IndexAsync(AiAttachment attachment, AiConversation conversation, CancellationToken ct);
    Task<IReadOnlyList<AiDocumentSearchResult>> SearchAsync(Guid userId, Guid conversationId,
        string query, int maximumResults, CancellationToken ct);
}

public interface ISmartProductService
{
    Task<AiConversationDto> CreateConversationAsync(AiUserContext user, CreateAiConversationRequest request, CancellationToken ct);
    Task<IReadOnlyList<AiConversationListItemDto>> SearchConversationsAsync(AiUserContext user, string? query, bool includeArchived, CancellationToken ct);
    Task<AiConversationDto> GetConversationAsync(AiUserContext user, Guid conversationId, CancellationToken ct);
    Task<AiConversationDto> UpdateConversationAsync(AiUserContext user, Guid conversationId, UpdateAiConversationRequest request, CancellationToken ct);
    Task DeleteConversationAsync(AiUserContext user, Guid conversationId, CancellationToken ct);
    IAsyncEnumerable<AiStreamEvent> SendMessageAsync(AiUserContext user, Guid conversationId, SendAiMessageRequest request, CancellationToken ct);
    IAsyncEnumerable<AiStreamEvent> RegenerateAsync(AiUserContext user, Guid messageId, CancellationToken ct);
    Task<AiAttachmentDto> UploadAsync(AiUserContext user, Guid conversationId, Stream content,
        string fileName, string mimeType, long length, CancellationToken ct);
    Task SaveFeedbackAsync(AiUserContext user, Guid messageId, AiFeedbackRequest request, CancellationToken ct);
}
