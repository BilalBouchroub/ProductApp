using FluentValidation;
using MediatR;

namespace ProductApp.Application.Auth;

public sealed record AuthenticatedUserDto(Guid Id, string Email, string FullName, string Role,
    IReadOnlyList<string> Permissions, bool MustChangePassword);
public sealed record TokenPairDto(string AccessToken, string RefreshToken, DateTime ExpiresAt,
    AuthenticatedUserDto User);

public interface IAuthService
{
    Task<TokenPairDto> LoginAsync(string email, string password, string ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task<TokenPairDto> RefreshAsync(string refreshToken, string ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken);
    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
public interface IPasswordResetNotifier
{ Task SendResetTokenAsync(string email, string token, CancellationToken cancellationToken); }

public sealed class AuthenticationException(string message) : Exception(message);
public sealed class InvalidRefreshTokenException() : Exception("The refresh token is invalid or expired.");

public sealed record LoginCommand(string Email, string Password, string IpAddress, string? UserAgent) : IRequest<TokenPairDto>;
public sealed record RefreshTokenCommand(string RefreshToken, string IpAddress, string? UserAgent) : IRequest<TokenPairDto>;
public sealed record LogoutCommand(string RefreshToken, string IpAddress) : IRequest;
public sealed record ForgotPasswordCommand(string Email) : IRequest;
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest;
public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest;
public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<AuthenticatedUserDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress(); RuleFor(x => x.Password).NotEmpty(); }
}
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress(); RuleFor(x => x.Token).NotEmpty(); RuleFor(x => x.NewPassword).SetValidator(new StrongPasswordValidator()); }
}
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator() { RuleFor(x => x.UserId).NotEmpty(); RuleFor(x => x.CurrentPassword).NotEmpty(); RuleFor(x => x.NewPassword).SetValidator(new StrongPasswordValidator()); }
}
public sealed class StrongPasswordValidator : AbstractValidator<string>
{
    public StrongPasswordValidator()
    {
        RuleFor(x => x).NotEmpty().MinimumLength(12).MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
    }
}

public sealed class LoginCommandHandler(IAuthService service) : IRequestHandler<LoginCommand, TokenPairDto>
{ public Task<TokenPairDto> Handle(LoginCommand r, CancellationToken c) => service.LoginAsync(r.Email, r.Password, r.IpAddress, r.UserAgent, c); }
public sealed class RefreshTokenCommandHandler(IAuthService service) : IRequestHandler<RefreshTokenCommand, TokenPairDto>
{ public Task<TokenPairDto> Handle(RefreshTokenCommand r, CancellationToken c) => service.RefreshAsync(r.RefreshToken, r.IpAddress, r.UserAgent, c); }
public sealed class LogoutCommandHandler(IAuthService service) : IRequestHandler<LogoutCommand>
{ public Task Handle(LogoutCommand r, CancellationToken c) => service.LogoutAsync(r.RefreshToken, r.IpAddress, c); }
public sealed class ForgotPasswordCommandHandler(IAuthService service) : IRequestHandler<ForgotPasswordCommand>
{ public Task Handle(ForgotPasswordCommand r, CancellationToken c) => service.ForgotPasswordAsync(r.Email, c); }
public sealed class ResetPasswordCommandHandler(IAuthService service) : IRequestHandler<ResetPasswordCommand>
{ public Task Handle(ResetPasswordCommand r, CancellationToken c) => service.ResetPasswordAsync(r.Email, r.Token, r.NewPassword, c); }
public sealed class ChangePasswordCommandHandler(IAuthService service) : IRequestHandler<ChangePasswordCommand>
{ public Task Handle(ChangePasswordCommand r, CancellationToken c) => service.ChangePasswordAsync(r.UserId, r.CurrentPassword, r.NewPassword, c); }
public sealed class GetCurrentUserQueryHandler(IAuthService service) : IRequestHandler<GetCurrentUserQuery, AuthenticatedUserDto>
{ public Task<AuthenticatedUserDto> Handle(GetCurrentUserQuery r, CancellationToken c) => service.GetCurrentUserAsync(r.UserId, c); }

public static class AppPermissions
{
    public const string ManageUsers = "users.manage";
    public const string ReadAudit = "audit.read";
    public const string ManageProducts = "products.manage";
    public const string ManageExperiments = "experiments.manage";
    public const string ManageStudies = "studies.manage";
    public static readonly IReadOnlyList<string> All = [ManageUsers, ReadAudit, ManageProducts, ManageExperiments, ManageStudies];
}

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string ProductionManager = "ProductionManager";
    public const string CommercialManager = "CommercialManager";
}
