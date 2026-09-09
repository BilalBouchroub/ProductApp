using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Administration;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;
using ProductApp.Domain.Identity;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Administration;

public sealed class AdministrationRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider, IUserInvitationSender invitationSender)
    : IAdministrationRepository
{
    public async Task<PagedResult<UserDto>> GetUsersAsync(PageRequest page, string? role, UserStatus? status, CancellationToken ct)
    {
        var query = db.ApplicationUsers.AsNoTracking().Include(x => x.Role)
            .Where(x => x.Status != UserStatus.Deleted);
        if (!string.IsNullOrWhiteSpace(page.Search))
        { var search = page.Search.Trim().ToLower(); query = query.Where(x => x.FirstName.ToLower().Contains(search) || x.LastName.ToLower().Contains(search) || x.Email.ToLower().Contains(search)); }
        if (!string.IsNullOrWhiteSpace(role)) query = query.Where(x => x.Role != null && x.Role.Name == role);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        query = (page.SortBy?.ToLowerInvariant(), page.Descending) switch
        {
            ("email", false) => query.OrderBy(x => x.Email),
            ("email", true) => query.OrderByDescending(x => x.Email),
            ("role", false) => query.OrderBy(x => x.Role!.Name),
            ("role", true) => query.OrderByDescending(x => x.Role!.Name),
            ("status", false) => query.OrderBy(x => x.Status),
            ("status", true) => query.OrderByDescending(x => x.Status),
            ("createdat", false) => query.OrderBy(x => x.CreatedAt),
            ("createdat", true) => query.OrderByDescending(x => x.CreatedAt),
            (_, true) => query.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName),
            _ => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
        };
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize)
            .Select(x => new UserDto(x.Id, x.FirstName, x.LastName, (x.FirstName + " " + x.LastName).Trim(), x.Email,
                x.PhoneNumber, x.Role == null ? string.Empty : x.Role.Name, x.Status, x.CreatedAt, x.UpdatedAt, x.LastLoginAt)).ToListAsync(ct);
        return new(items, page.SafePage, page.SafePageSize, total);
    }

    public Task<UserDto?> GetUserAsync(Guid id, CancellationToken ct) => db.ApplicationUsers.AsNoTracking()
        .Where(x => x.Id == id && x.Status != UserStatus.Deleted)
        .Select(x => new UserDto(x.Id, x.FirstName, x.LastName, (x.FirstName + " " + x.LastName).Trim(), x.Email,
            x.PhoneNumber, x.Role == null ? string.Empty : x.Role.Name, x.Status, x.CreatedAt, x.UpdatedAt, x.LastLoginAt)).SingleOrDefaultAsync(ct);

    public async Task<UserDto> CreateUserAsync(string firstName, string lastName, string email, string? phone, string roleName, string temporaryPassword, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(email) is not null) throw new AdministrationConflictException("A user with this email already exists.");
        var role = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == roleName.ToUpper(), ct)
            ?? throw new AdministrationNotFoundException("Role");
        var user = ApplicationUser.Create(firstName, lastName, email, role.Id, timeProvider.GetUtcNow().UtcDateTime);
        user.UpdateProfile(firstName, lastName, phone);
        var result = await userManager.CreateAsync(user, temporaryPassword);
        EnsureSuccess(result);
        try
        {
            await invitationSender.SendAsync(user.Email,
                $"{user.FirstName} {user.LastName}".Trim(), role.Name, temporaryPassword, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var cleanup = await userManager.DeleteAsync(user);
            if (!cleanup.Succeeded)
                throw new AdministrationConflictException(
                    "L’envoi de l’invitation a échoué et le compte créé n’a pas pu être annulé. Vérifiez la configuration SMTP et supprimez ce compte manuellement.");
            throw new InvitationDeliveryException(
                "Le compte n’a pas été créé car le serveur SMTP n’a pas pu envoyer l’e-mail. Vérifiez les identifiants SMTP et les variables du backend.",
                exception);
        }
        return (await GetUserAsync(user.Id, ct))!;
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, string firstName, string lastName, string? phone, CancellationToken ct)
    { var user = await FindUserAsync(id, ct); user.UpdateProfile(firstName, lastName, phone); EnsureSuccess(await userManager.UpdateAsync(user)); return (await GetUserAsync(id, ct))!; }
    public async Task<UserDto> ChangeRoleAsync(Guid id, string roleName, CancellationToken ct)
    { var user = await FindUserAsync(id, ct); var role = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == roleName.ToUpper(), ct) ?? throw new AdministrationNotFoundException("Role"); user.AssignRole(role.Id); EnsureSuccess(await userManager.UpdateAsync(user)); return (await GetUserAsync(id, ct))!; }
    public async Task<UserDto> ChangeStatusAsync(Guid id, UserStatus status, CancellationToken ct)
    {
        var user = await FindUserAsync(id, ct);
        user.SetStatus(status);
        if (status != UserStatus.Active) await RevokeRefreshTokensAsync(user.Id, "status-change", ct);
        EnsureSuccess(await userManager.UpdateAsync(user));
        return (await GetUserAsync(id, ct))!;
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct)
    {
        var user = await FindUserAsync(id, ct);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await RevokeRefreshTokensAsync(user.Id, "account-deleted", ct);
        user.MarkDeleted(now);
        EnsureSuccess(await userManager.UpdateAsync(user));
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct) => await db.Roles.AsNoTracking().OrderBy(x => x.Name)
        .Select(x => new RoleDto(x.Id, x.Name, x.Description, x.RolePermissions.Select(p => p.Permission.Code).OrderBy(p => p).ToList())).ToListAsync(ct);

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, PageRequest page, bool? isRead, CancellationToken ct)
    {
        var query = db.Notifications.AsNoTracking().Where(x => x.UserId == userId);
        if (isRead.HasValue) query = query.Where(x => x.IsRead == isRead);
        if (!string.IsNullOrWhiteSpace(page.Search)) { var search = page.Search.Trim().ToLower(); query = query.Where(x => x.Title.ToLower().Contains(search) || x.Message.ToLower().Contains(search)); }
        query = page.Descending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize)
            .Select(x => new NotificationDto(x.Id, x.Title, x.Message, x.Type, x.IsRead, x.CreatedAt, x.RelatedEntityType, x.RelatedEntityId)).ToListAsync(ct);
        return new(items, page.SafePage, page.SafePageSize, total);
    }
    public async Task MarkNotificationReadAsync(Guid userId, Guid notificationId, CancellationToken ct)
    { var notification = await db.Notifications.SingleOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct) ?? throw new AdministrationNotFoundException("Notification"); notification.MarkRead(timeProvider.GetUtcNow().UtcDateTime, userId); await db.SaveChangesAsync(ct); }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(PageRequest page, string? module, AuditLevel? level, Guid? userId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var query = db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(page.Search)) { var search = page.Search.Trim().ToLower(); query = query.Where(x => x.Action.ToLower().Contains(search) || x.Description.ToLower().Contains(search)); }
        if (!string.IsNullOrWhiteSpace(module)) query = query.Where(x => x.Module == module);
        if (level.HasValue) query = query.Where(x => x.Level == level); if (userId.HasValue) query = query.Where(x => x.UserId == userId);
        if (from.HasValue) query = query.Where(x => x.Timestamp >= from); if (to.HasValue) query = query.Where(x => x.Timestamp <= to);
        query = (page.SortBy?.ToLowerInvariant(), page.Descending) switch
        { ("action", false) => query.OrderBy(x => x.Action), ("action", true) => query.OrderByDescending(x => x.Action), ("level", false) => query.OrderBy(x => x.Level), ("level", true) => query.OrderByDescending(x => x.Level), (_, false) => query.OrderByDescending(x => x.Timestamp), _ => query.OrderBy(x => x.Timestamp) };
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page.SafePage - 1) * page.SafePageSize).Take(page.SafePageSize)
            .Select(x => new AuditLogDto(x.Id, x.Timestamp, x.UserId, x.User == null ? null : (x.User.FirstName + " " + x.User.LastName).Trim(), x.Action, x.Module, x.Description, x.Level, x.IpAddress, x.EntityType, x.EntityId)).ToListAsync(ct);
        return new(items, page.SafePage, page.SafePageSize, total);
    }

    private async Task RevokeRefreshTokensAsync(Guid userId, string reason, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now).ToListAsync(ct);
        foreach (var token in tokens) token.Revoke(now, reason);
    }

    private async Task<ApplicationUser> FindUserAsync(Guid id, CancellationToken ct) => await db.ApplicationUsers
        .SingleOrDefaultAsync(x => x.Id == id && x.Status != UserStatus.Deleted, ct)
        ?? throw new AdministrationNotFoundException("User");
    private static void EnsureSuccess(IdentityResult result) { if (!result.Succeeded) throw new AdministrationConflictException(string.Join(" ", result.Errors.Select(x => x.Description))); }
}
