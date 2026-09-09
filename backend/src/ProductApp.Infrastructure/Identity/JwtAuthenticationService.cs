using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Application.Auth;
using ProductApp.Domain.Common;
using ProductApp.Domain.Identity;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "ProductApp.Api";
    public string Audience { get; init; } = "ProductApp.Client";
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}

public static class ProductAppClaimTypes { public const string Permission = "permission"; }

public sealed class JwtAuthenticationService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IOptions<JwtOptions> options,
    IPasswordResetNotifier resetNotifier,
    TimeProvider timeProvider) : IAuthService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<TokenPairDto> LoginAsync(string email, string password, string ipAddress, string? userAgent, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || user.Status != UserStatus.Active || !await userManager.CheckPasswordAsync(user, password))
        {
            if (user is not null) await userManager.AccessFailedAsync(user);
            throw new AuthenticationException("Invalid email or password.");
        }
        if (await userManager.IsLockedOutAsync(user)) throw new AuthenticationException("Account temporarily locked.");
        await userManager.ResetAccessFailedCountAsync(user);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.RecordLogin(now);
        db.AuditLogs.Add(AuditLog.Create(user.Id, "Login", "Successful user login.", ipAddress, now));
        return await IssueTokenPairAsync(user, ipAddress, userAgent, now, ct);
    }

    public async Task<TokenPairDto> RefreshAsync(string refreshToken, string ipAddress, string? userAgent, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is null || !stored.IsActive(now) || stored.User.Status != UserStatus.Active)
            throw new InvalidRefreshTokenException();
        var replacementRaw = GenerateRefreshToken();
        var replacementHash = Hash(replacementRaw);
        stored.Revoke(now, ipAddress, replacementHash);
        var replacement = RefreshToken.Create(stored.UserId, replacementHash, now.AddDays(_options.RefreshTokenDays), ipAddress, userAgent, now);
        db.RefreshTokens.Add(replacement);
        var result = await BuildTokenPairAsync(stored.User, replacementRaw, now, ct);
        await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken ct)
    {
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == Hash(refreshToken), ct);
        if (stored is null) return;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        stored.Revoke(now, ipAddress);
        db.AuditLogs.Add(AuditLog.Create(stored.UserId, "Logout", "User session revoked.", ipAddress, now));
        await db.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.EmailConfirmed) return;
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await resetNotifier.SendResetTokenAsync(user.Email, token, ct);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email) ?? throw new AuthenticationException("Password reset request is invalid.");
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        EnsureSuccess(result);
        user.PasswordChanged();
        await RevokeAllAsync(user.Id, ct);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new AuthenticationException("User not found.");
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        EnsureSuccess(result);
        user.PasswordChanged();
        await RevokeAllAsync(user.Id, ct);
    }

    public async Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.ApplicationUsers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new AuthenticationException("User not found.");
        return await MapUserAsync(user, ct);
    }

    private async Task<TokenPairDto> IssueTokenPairAsync(ApplicationUser user, string ip, string? agent, DateTime now, CancellationToken ct)
    {
        var raw = GenerateRefreshToken();
        db.RefreshTokens.Add(RefreshToken.Create(user.Id, Hash(raw), now.AddDays(_options.RefreshTokenDays), ip, agent, now));
        var result = await BuildTokenPairAsync(user, raw, now, ct);
        await db.SaveChangesAsync(ct);
        return result;
    }

    private async Task<TokenPairDto> BuildTokenPairAsync(ApplicationUser user, string refreshToken, DateTime now, CancellationToken ct)
    {
        if (_options.SigningKey.Length < 32) throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 characters.");
        var mapped = await MapUserAsync(user, ct);
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")), new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, mapped.FullName), new(ClaimTypes.Role, mapped.Role)
        };
        claims.AddRange(mapped.Permissions.Select(x => new Claim(ProductAppClaimTypes.Permission, x)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, now, expires, credentials);
        return new TokenPairDto(new JwtSecurityTokenHandler().WriteToken(jwt), refreshToken, expires, mapped);
    }

    private async Task<AuthenticatedUserDto> MapUserAsync(ApplicationUser user, CancellationToken ct)
    {
        var role = user.RoleId.HasValue ? await db.Roles.AsNoTracking().Where(x => x.Id == user.RoleId).Select(x => x.Name).SingleAsync(ct) : string.Empty;
        var permissions = user.RoleId.HasValue ? await db.RolePermissions.AsNoTracking().Where(x => x.RoleId == user.RoleId)
            .Select(x => x.Permission.Code).OrderBy(x => x).ToListAsync(ct) : [];
        return new(user.Id, user.Email, $"{user.FirstName} {user.LastName}".Trim(), role, permissions, user.MustChangePassword);
    }

    private async Task RevokeAllAsync(Guid userId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now).ToListAsync(ct);
        foreach (var token in tokens) token.Revoke(now, "password-change");
        await db.SaveChangesAsync(ct);
    }
    private static string GenerateRefreshToken() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static void EnsureSuccess(IdentityResult result)
    { if (!result.Succeeded) throw new AuthenticationException(string.Join(" ", result.Errors.Select(x => x.Description))); }
}

public sealed class LoggingPasswordResetNotifier(ILogger<LoggingPasswordResetNotifier> logger) : IPasswordResetNotifier
{
    public Task SendResetTokenAsync(string email, string token, CancellationToken cancellationToken)
    {
        logger.LogInformation("A password reset token was generated for {Email}. Configure an email provider to deliver it.", email);
        return Task.CompletedTask;
    }
}
