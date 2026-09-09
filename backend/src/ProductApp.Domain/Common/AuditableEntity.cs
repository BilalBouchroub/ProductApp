namespace ProductApp.Domain.Common;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected void InitializeAudit(DateTime createdAt, Guid? actorId = null)
    {
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CreatedBy = actorId;
        UpdatedBy = actorId;
    }

    protected void Touch(DateTime updatedAt, Guid? actorId = null)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = actorId;
    }
}
