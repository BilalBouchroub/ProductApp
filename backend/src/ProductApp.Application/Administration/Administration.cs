using FluentValidation;
using MediatR;
using ProductApp.Application.Auth;
using ProductApp.Application.Common;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Administration;

public sealed record UserDto(Guid Id, string FirstName, string LastName, string FullName, string Email,
    string? PhoneNumber, string Role, UserStatus Status, DateTime CreatedAt, DateTime UpdatedAt, DateTime? LastLoginAt);
public sealed record RoleDto(Guid Id, string Name, string Description, IReadOnlyList<string> Permissions);
public sealed record NotificationDto(Guid Id, string Title, string Message, NotificationType Type, bool IsRead,
    DateTime CreatedAt, string? RelatedEntityType, Guid? RelatedEntityId);
public sealed record AuditLogDto(Guid Id, DateTime Timestamp, Guid? UserId, string? UserName, string Action,
    string Module, string Description, AuditLevel Level, string? IpAddress, string? EntityType, Guid? EntityId);
public enum UserAdministrationAction { Activate, Deactivate, Suspend, ChangeRole, Delete, ResetPassword }
public sealed record UserActionResultDto(bool Deleted, UserDto? User);

public interface IAdministrationRepository
{
    Task<PagedResult<UserDto>> GetUsersAsync(PageRequest page, string? role, UserStatus? status, CancellationToken ct);
    Task<UserDto?> GetUserAsync(Guid id, CancellationToken ct);
    Task<UserDto> CreateUserAsync(string firstName, string lastName, string email, string? phone, string role, string temporaryPassword, CancellationToken ct);
    Task<UserDto> UpdateUserAsync(Guid id, string firstName, string lastName, string? phone, CancellationToken ct);
    Task<UserDto> ChangeRoleAsync(Guid id, string role, CancellationToken ct);
    Task<UserDto> ChangeStatusAsync(Guid id, UserStatus status, CancellationToken ct);
    Task DeleteUserAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct);
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, PageRequest page, bool? isRead, CancellationToken ct);
    Task MarkNotificationReadAsync(Guid userId, Guid notificationId, CancellationToken ct);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(PageRequest page, string? module, AuditLevel? level, Guid? userId, DateTime? from, DateTime? to, CancellationToken ct);
}

public interface IUserInvitationSender
{
    Task SendAsync(string email, string fullName, string role, string temporaryPassword,
        CancellationToken cancellationToken);
}

public sealed class AdministrationNotFoundException(string resource) : Exception($"{resource} was not found.");
public sealed class AdministrationConflictException(string message) : Exception(message);
public sealed class InvitationDeliveryException(string message, Exception innerException)
    : Exception(message, innerException);

public sealed record GetUsersQuery(PageRequest Page, string? Role, UserStatus? Status) : IRequest<PagedResult<UserDto>>;
public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;
public sealed record CreateUserCommand(string FirstName, string LastName, string Email, string? PhoneNumber, string Role, string TemporaryPassword) : IRequest<UserDto>;
public sealed record UpdateUserCommand(Guid Id, string FirstName, string LastName, string? PhoneNumber) : IRequest<UserDto>;
public sealed record ExecuteUserActionCommand(Guid Id, Guid ActorId, UserAdministrationAction Action, string? Role = null)
    : IRequest<UserActionResultDto>;
public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleDto>>;
public sealed record GetNotificationsQuery(Guid UserId, PageRequest Page, bool? IsRead) : IRequest<PagedResult<NotificationDto>>;
public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest;
public sealed record GetAuditLogsQuery(PageRequest Page, string? Module, AuditLevel? Level, Guid? UserId, DateTime? From, DateTime? To) : IRequest<PagedResult<AuditLogDto>>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100); RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320); RuleFor(x => x.Role).NotEmpty();
        RuleFor(x => x.TemporaryPassword).NotEmpty().MinimumLength(12).Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]").Matches("[^a-zA-Z0-9]");
    }
}
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{ public UpdateUserCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100); RuleFor(x => x.LastName).NotEmpty().MaximumLength(100); RuleFor(x => x.PhoneNumber).MaximumLength(30); } }
public sealed class ExecuteUserActionCommandValidator : AbstractValidator<ExecuteUserActionCommand>
{
    public ExecuteUserActionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Action).IsInEnum();
        RuleFor(x => x.Role)
            .Must(role => role is "Administrator" or "ProductionManager" or "CommercialManager")
            .When(x => x.Action == UserAdministrationAction.ChangeRole)
            .WithMessage("Un rôle valide est obligatoire pour cette action.");
    }
}
public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public sealed class GetUsersHandler(IAdministrationRepository r) : IRequestHandler<GetUsersQuery, PagedResult<UserDto>> { public Task<PagedResult<UserDto>> Handle(GetUsersQuery q, CancellationToken c) => r.GetUsersAsync(q.Page, q.Role, q.Status, c); }
public sealed class GetUserHandler(IAdministrationRepository r) : IRequestHandler<GetUserQuery, UserDto> { public async Task<UserDto> Handle(GetUserQuery q, CancellationToken c) => await r.GetUserAsync(q.Id, c) ?? throw new AdministrationNotFoundException("User"); }
public sealed class CreateUserHandler(IAdministrationRepository r) : IRequestHandler<CreateUserCommand, UserDto> { public Task<UserDto> Handle(CreateUserCommand q, CancellationToken c) => r.CreateUserAsync(q.FirstName, q.LastName, q.Email, q.PhoneNumber, q.Role, q.TemporaryPassword, c); }
public sealed class UpdateUserHandler(IAdministrationRepository r) : IRequestHandler<UpdateUserCommand, UserDto> { public Task<UserDto> Handle(UpdateUserCommand q, CancellationToken c) => r.UpdateUserAsync(q.Id, q.FirstName, q.LastName, q.PhoneNumber, c); }
public sealed class ExecuteUserActionHandler(IAdministrationRepository repository, IAuthService authService)
    : IRequestHandler<ExecuteUserActionCommand, UserActionResultDto>
{
    public async Task<UserActionResultDto> Handle(ExecuteUserActionCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == command.ActorId && command.Action is UserAdministrationAction.Deactivate
            or UserAdministrationAction.Suspend or UserAdministrationAction.Delete)
            throw new AdministrationConflictException("Vous ne pouvez pas désactiver, suspendre ou supprimer votre propre compte.");

        UserDto? user = command.Action switch
        {
            UserAdministrationAction.Activate => await repository.ChangeStatusAsync(command.Id, UserStatus.Active, cancellationToken),
            UserAdministrationAction.Deactivate => await repository.ChangeStatusAsync(command.Id, UserStatus.Inactive, cancellationToken),
            UserAdministrationAction.Suspend => await repository.ChangeStatusAsync(command.Id, UserStatus.Suspended, cancellationToken),
            UserAdministrationAction.ChangeRole => await repository.ChangeRoleAsync(command.Id, command.Role!, cancellationToken),
            UserAdministrationAction.ResetPassword => await SendPasswordResetAsync(command.Id, cancellationToken),
            UserAdministrationAction.Delete => null,
            _ => throw new ArgumentOutOfRangeException(nameof(command.Action))
        };

        if (command.Action == UserAdministrationAction.Delete)
        {
            await repository.DeleteUserAsync(command.Id, cancellationToken);
            return new(true, null);
        }

        return new(false, user);
    }

    private async Task<UserDto> SendPasswordResetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserAsync(id, cancellationToken)
            ?? throw new AdministrationNotFoundException("User");
        await authService.ForgotPasswordAsync(user.Email, cancellationToken);
        return user;
    }
}
public sealed class GetRolesHandler(IAdministrationRepository r) : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleDto>> { public Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery q, CancellationToken c) => r.GetRolesAsync(c); }
public sealed class GetNotificationsHandler(IAdministrationRepository r) : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>> { public Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery q, CancellationToken c) => r.GetNotificationsAsync(q.UserId, q.Page, q.IsRead, c); }
public sealed class MarkNotificationReadHandler(IAdministrationRepository r) : IRequestHandler<MarkNotificationReadCommand> { public Task Handle(MarkNotificationReadCommand q, CancellationToken c) => r.MarkNotificationReadAsync(q.UserId, q.NotificationId, c); }
public sealed class GetAuditLogsHandler(IAdministrationRepository r) : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>> { public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery q, CancellationToken c) => r.GetAuditLogsAsync(q.Page, q.Module, q.Level, q.UserId, q.From, q.To, c); }
