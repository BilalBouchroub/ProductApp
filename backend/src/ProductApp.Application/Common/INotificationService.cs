using ProductApp.Domain.Common;

namespace ProductApp.Application.Common;

public interface INotificationService
{
    Task NotifyRoleAsync(string role, string title, string message, NotificationType type,
        string? relatedEntityType, Guid? relatedEntityId, CancellationToken cancellationToken);
}
