using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;
using ProductApp.Domain.Identity;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Administration;

public sealed class NotificationService(ApplicationDbContext dbContext, TimeProvider timeProvider)
    : INotificationService
{
    public async Task NotifyRoleAsync(string role, string title, string message,
        NotificationType type, string? relatedEntityType, Guid? relatedEntityId,
        CancellationToken cancellationToken)
    {
        var userIds = await dbContext.ApplicationUsers.AsNoTracking()
            .Where(user => user.Status == UserStatus.Active && user.Role != null && user.Role.Name == role)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.Notifications.AddRange(userIds.Select(userId => Notification.Create(userId,
            title, message, type, relatedEntityType, relatedEntityId, now)));
    }
}
