using ProductApp.Domain.Common;

namespace ProductApp.Domain.Identity;

public sealed class ApplicationUser : AuditableEntity
{
    private ApplicationUser() { }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string NormalizedUserName { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string SecurityStamp { get; private set; } = string.Empty;
    public string ConcurrencyStamp { get; private set; } = string.Empty;
    public bool TwoFactorEnabled { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; }
    public int AccessFailedCount { get; private set; }
    public bool MustChangePassword { get; private set; }
    public UserStatus Status { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public Guid? RoleId { get; private set; }
    public Role? Role { get; private set; }
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public static ApplicationUser Create(string firstName, string lastName, string email, Guid roleId, DateTime utcNow)
    {
        var normalized = email.Trim().ToUpperInvariant();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim(),
            NormalizedEmail = normalized,
            UserName = email.Trim(),
            NormalizedUserName = normalized,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            Status = UserStatus.Active,
            RoleId = roleId,
            LockoutEnabled = true,
            MustChangePassword = true
        };
        user.InitializeAudit(utcNow);
        return user;
    }

    public void SetUserName(string? value) { UserName = value?.Trim() ?? string.Empty; Touch(DateTime.UtcNow); }
    public void SetNormalizedUserName(string? value) { NormalizedUserName = value ?? string.Empty; }
    public void SetEmail(string? value) { Email = value?.Trim() ?? string.Empty; Touch(DateTime.UtcNow); }
    public void SetNormalizedEmail(string? value) { NormalizedEmail = value ?? string.Empty; }
    public void SetEmailConfirmed(bool value) { EmailConfirmed = value; Touch(DateTime.UtcNow); }
    public void SetPhoneNumber(string? value) { PhoneNumber = value?.Trim(); Touch(DateTime.UtcNow); }
    public void SetPhoneNumberConfirmed(bool value) { PhoneNumberConfirmed = value; Touch(DateTime.UtcNow); }
    public void SetPasswordHash(string? value) { PasswordHash = value ?? string.Empty; Touch(DateTime.UtcNow); }
    public void SetSecurityStamp(string value) { SecurityStamp = value; Touch(DateTime.UtcNow); }
    public void SetConcurrencyStamp(string value) { ConcurrencyStamp = value; }
    public void SetTwoFactorEnabled(bool value) { TwoFactorEnabled = value; Touch(DateTime.UtcNow); }
    public void SetLockoutEnd(DateTimeOffset? value) { LockoutEnd = value; }
    public void SetLockoutEnabled(bool value) { LockoutEnabled = value; }
    public void SetAccessFailedCount(int value) { AccessFailedCount = value; }
    public void AssignRole(Guid roleId) { RoleId = roleId; Touch(DateTime.UtcNow); }
    public void ClearRole() { RoleId = null; Touch(DateTime.UtcNow); }
    public void PasswordChanged() { MustChangePassword = false; SecurityStamp = Guid.NewGuid().ToString("N"); Touch(DateTime.UtcNow); }
    public void RecordLogin(DateTime utcNow) { LastLoginAt = utcNow; Touch(utcNow, Id); }
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    { FirstName = firstName.Trim(); LastName = lastName.Trim(); PhoneNumber = phoneNumber?.Trim(); Touch(DateTime.UtcNow); }
    public void SetStatus(UserStatus status) { Status = status; Touch(DateTime.UtcNow); }
    public void MarkDeleted(DateTime utcNow)
    {
        var deletedEmail = $"deleted-{Id:N}@productapp.invalid";
        FirstName = "Utilisateur";
        LastName = "Supprime";
        Email = deletedEmail;
        NormalizedEmail = deletedEmail.ToUpperInvariant();
        UserName = deletedEmail;
        NormalizedUserName = NormalizedEmail;
        PhoneNumber = null;
        PasswordHash = "DELETED";
        SecurityStamp = Guid.NewGuid().ToString("N");
        RoleId = null;
        Status = UserStatus.Deleted;
        MustChangePassword = false;
        Touch(utcNow);
    }
}
