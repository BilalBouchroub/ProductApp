using Microsoft.EntityFrameworkCore;
using ProductApp.Application.SmartProduct;
using ProductApp.Domain.SmartProduct;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed class AiConversationRepository(ApplicationDbContext db) : IAiConversationRepository
{
    public Task<AiConversation?> GetOwnedAsync(Guid conversationId, Guid userId, bool includeDetails, CancellationToken ct)
    {
        IQueryable<AiConversation> query = db.AiConversations;
        if (includeDetails) query = query.AsSplitQuery().Include(x => x.Messages).ThenInclude(x => x.Sources).Include(x => x.Attachments);
        return query.SingleOrDefaultAsync(x => x.Id == conversationId && x.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<AiConversation>> SearchOwnedAsync(Guid userId, string? query, bool includeArchived, CancellationToken ct)
    {
        IQueryable<AiConversation> items = db.AiConversations.AsNoTracking().Include(x => x.Messages)
            .Where(x => x.UserId == userId && (includeArchived || !x.IsArchived));
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            items = items.Where(x => x.Title.Contains(term) || x.Messages.Any(m => m.Content.Contains(term))
                || (x.Product != null && (x.Product.Code.Contains(term) || x.Product.Name.Contains(term)))
                || (x.Experiment != null && x.Experiment.Name.Contains(term)));
        }
        return await items.OrderByDescending(x => x.LastMessageAt).Take(200).ToListAsync(ct);
    }

    public Task<AiMessage?> GetOwnedMessageAsync(Guid messageId, Guid userId, CancellationToken ct) =>
        db.AiMessages.Include(x => x.Conversation).SingleOrDefaultAsync(x => x.Id == messageId && x.Conversation.UserId == userId, ct);
    public Task<AiAttachment?> GetOwnedAttachmentAsync(Guid attachmentId, Guid userId, CancellationToken ct) =>
        db.AiAttachments.SingleOrDefaultAsync(x => x.Id == attachmentId && x.UserId == userId, ct);
    public Task AddConversationAsync(AiConversation item, CancellationToken ct) => db.AiConversations.AddAsync(item, ct).AsTask();
    public Task AddMessageAsync(AiMessage item, CancellationToken ct) => db.AiMessages.AddAsync(item, ct).AsTask();
    public Task AddAttachmentAsync(AiAttachment item, CancellationToken ct) => db.AiAttachments.AddAsync(item, ct).AsTask();
    public Task AddSourcesAsync(IEnumerable<AiMessageSource> items, CancellationToken ct) => db.AiMessageSources.AddRangeAsync(items, ct);
    public Task AddFeedbackAsync(AiFeedback item, CancellationToken ct) => db.AiFeedback.AddAsync(item, ct).AsTask();
    public Task AddToolInvocationAsync(AiToolInvocation item, CancellationToken ct) => db.AiToolInvocations.AddAsync(item, ct).AsTask();
    public async Task ReplaceChunksAsync(Guid attachmentId, IEnumerable<AiDocumentChunk> chunks, CancellationToken ct)
    {
        var existing = await db.AiDocumentChunks.Where(x => x.AttachmentId == attachmentId).ToListAsync(ct);
        db.AiDocumentChunks.RemoveRange(existing); await db.AiDocumentChunks.AddRangeAsync(chunks, ct);
    }
    public async Task<IReadOnlyList<AiDocumentChunk>> GetAuthorizedChunksAsync(Guid userId, Guid conversationId, CancellationToken ct) =>
        await db.AiDocumentChunks.AsNoTracking().Include(x => x.Attachment)
            .Where(x => x.UserId == userId && x.ConversationId == conversationId).Take(2000).ToListAsync(ct);
    public void RemoveConversation(AiConversation item) => db.AiConversations.Remove(item);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
