using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domain.SmartProduct;

namespace ProductApp.Infrastructure.Persistence.Configurations;

public sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("AiConversations"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Summary).HasColumnType("nvarchar(max)");
        builder.Property(x => x.LastMessageAt).HasColumnType("datetime2");
        builder.HasIndex(x => new { x.UserId, x.IsArchived, x.LastMessageAt });
        builder.HasIndex(x => x.ProductId); builder.HasIndex(x => x.ExperimentId);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Experiment).WithMany().HasForeignKey(x => x.ExperimentId).OnDelete(DeleteBehavior.NoAction);
        builder.ConfigureAudit();
    }
}

public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("AiMessages"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasConversion<int>(); builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Model).HasMaxLength(120); builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.StructuredContentJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt });
        builder.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AiMessageSourceConfiguration : IEntityTypeConfiguration<AiMessageSource>
{
    public void Configure(EntityTypeBuilder<AiMessageSource> builder)
    {
        builder.ToTable("AiMessageSources"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasConversion<int>(); builder.Property(x => x.Label).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(300); builder.Property(x => x.InternalUrl).HasMaxLength(500);
        builder.Property(x => x.Excerpt).HasMaxLength(2000); builder.HasIndex(x => new { x.MessageId, x.Kind });
        builder.HasOne(x => x.Message).WithMany(x => x.Sources).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AiAttachmentConfiguration : IEntityTypeConfiguration<AiAttachment>
{
    public void Configure(EntityTypeBuilder<AiAttachment> builder)
    {
        builder.ToTable("AiAttachments"); builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired(); builder.Property(x => x.StoredFileName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired(); builder.Property(x => x.MimeType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired(); builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.ErrorCode).HasMaxLength(100); builder.HasIndex(x => new { x.UserId, x.ConversationId, x.CreatedAt });
        builder.HasOne(x => x.Conversation).WithMany(x => x.Attachments).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AiDocumentChunkConfiguration : IEntityTypeConfiguration<AiDocumentChunk>
{
    public void Configure(EntityTypeBuilder<AiDocumentChunk> builder)
    {
        builder.ToTable("AiDocumentChunks"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasMaxLength(500).IsRequired(); builder.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.UserId, x.ConversationId }); builder.HasIndex(x => new { x.AttachmentId, x.ChunkIndex }).IsUnique();
        builder.HasIndex(x => x.ProductId); builder.HasIndex(x => x.ExperimentId);
        builder.HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AiFeedbackConfiguration : IEntityTypeConfiguration<AiFeedback>
{
    public void Configure(EntityTypeBuilder<AiFeedback> builder)
    {
        builder.ToTable("AiFeedback"); builder.HasKey(x => x.Id); builder.Property(x => x.Rating).HasConversion<int>();
        builder.Property(x => x.Comment).HasMaxLength(2000); builder.HasIndex(x => new { x.MessageId, x.UserId }).IsUnique();
        builder.HasOne(x => x.Message).WithMany(x => x.Feedback).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class AiToolInvocationConfiguration : IEntityTypeConfiguration<AiToolInvocation>
{
    public void Configure(EntityTypeBuilder<AiToolInvocation> builder)
    {
        builder.ToTable("AiToolInvocations"); builder.HasKey(x => x.Id); builder.Property(x => x.ToolName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>(); builder.Property(x => x.SafeErrorCode).HasMaxLength(100);
        builder.Property(x => x.ArgumentsSummary).HasMaxLength(1000); builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => new { x.ConversationId, x.MessageId }); builder.ConfigureAudit();
    }
}
