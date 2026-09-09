using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken() { }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public string? RevokedByIp { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsActive(DateTime utcNow) => RevokedAt is null && ExpiresAt > utcNow;

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt,
        string ipAddress, string? userAgent, DateTime createdAt)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };
        token.InitializeAudit(createdAt, userId);
        return token;
    }

    public void Revoke(DateTime revokedAt, string ipAddress, string? replacementHash = null)
    {
        if (RevokedAt.HasValue) return;
        RevokedAt = revokedAt; RevokedByIp = ipAddress;
        ReplacedByTokenHash = replacementHash; Touch(revokedAt, UserId);
    }
}
