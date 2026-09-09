using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class AuditLog : AuditableEntity
{
    private AuditLog() { }
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid? UserId { get; private set; }
    public ApplicationUser? User { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AuditLevel Level { get; private set; }
    public string? IpAddress { get; private set; }
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? ChangesJson { get; private set; }

    public static AuditLog Create(Guid? userId, string action, string description, string? ipAddress, DateTime utcNow)
        => Create(userId, action, "Authentication", description, AuditLevel.Information,
            ipAddress, "ApplicationUser", userId, null, utcNow);

    public static AuditLog Create(Guid? userId, string action, string module, string description,
        AuditLevel level, string? ipAddress, string? entityType, Guid? entityId,
        string? correlationId, DateTime utcNow)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = utcNow,
            UserId = userId,
            Action = action,
            Module = module,
            Description = description,
            Level = level,
            IpAddress = ipAddress,
            EntityType = entityType,
            EntityId = entityId,
            CorrelationId = correlationId
        };
        log.InitializeAudit(utcNow, userId);
        return log;
    }
}
