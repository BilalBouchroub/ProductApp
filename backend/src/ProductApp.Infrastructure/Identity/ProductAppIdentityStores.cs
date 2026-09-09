using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductApp.Domain.Identity;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Identity;

public sealed class ProductAppUserStore(ApplicationDbContext db) :
    IQueryableUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>, IUserPhoneNumberStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>, IUserSecurityStampStore<ApplicationUser>,
    IUserLockoutStore<ApplicationUser>, IUserTwoFactorStore<ApplicationUser>
{
    public IQueryable<ApplicationUser> Users => db.ApplicationUsers;
    public void Dispose() { }
    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); db.ApplicationUsers.Add(user); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); user.SetConcurrencyStamp(Guid.NewGuid().ToString("N")); db.ApplicationUsers.Update(user); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); db.ApplicationUsers.Remove(user); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id.ToString());
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string? name, CancellationToken ct) { user.SetUserName(name); return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? name, CancellationToken ct) { user.SetNormalizedUserName(name); return Task.CompletedTask; }
    public Task<ApplicationUser?> FindByIdAsync(string id, CancellationToken ct) => Guid.TryParse(id, out var key)
        ? db.ApplicationUsers.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == key, ct) : Task.FromResult<ApplicationUser?>(null);
    public Task<ApplicationUser?> FindByNameAsync(string normalizedName, CancellationToken ct) => db.ApplicationUsers.Include(x => x.Role)
        .SingleOrDefaultAsync(x => x.NormalizedUserName == normalizedName, ct);
    public Task SetPasswordHashAsync(ApplicationUser user, string? hash, CancellationToken ct) { user.SetPasswordHash(hash); return Task.CompletedTask; }
    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.PasswordHash);
    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken ct) { user.SetEmail(email); return Task.CompletedTask; }
    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.Email);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.EmailConfirmed);
    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken ct) { user.SetEmailConfirmed(confirmed); return Task.CompletedTask; }
    public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) => db.ApplicationUsers.Include(x => x.Role)
        .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.NormalizedEmail);
    public Task SetNormalizedEmailAsync(ApplicationUser user, string? email, CancellationToken ct) { user.SetNormalizedEmail(email); return Task.CompletedTask; }
    public Task SetPhoneNumberAsync(ApplicationUser user, string? phone, CancellationToken ct) { user.SetPhoneNumber(phone); return Task.CompletedTask; }
    public Task<string?> GetPhoneNumberAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.PhoneNumber);
    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.PhoneNumberConfirmed);
    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken ct) { user.SetPhoneNumberConfirmed(confirmed); return Task.CompletedTask; }
    public async Task AddToRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken ct)
    { var role = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, ct) ?? throw new InvalidOperationException("Role not found."); user.AssignRole(role.Id); }
    public async Task RemoveFromRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken ct)
    { if (await IsInRoleAsync(user, normalizedRoleName, ct)) user.ClearRole(); }
    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct)
    { if (!user.RoleId.HasValue) return []; var name = await db.Roles.Where(x => x.Id == user.RoleId).Select(x => x.Name).SingleAsync(ct); return [name]; }
    public async Task<bool> IsInRoleAsync(ApplicationUser user, string normalizedRoleName, CancellationToken ct) => user.RoleId.HasValue &&
        await db.Roles.AnyAsync(x => x.Id == user.RoleId && x.NormalizedName == normalizedRoleName, ct);
    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken ct)
    { var roleId = await db.Roles.Where(x => x.NormalizedName == normalizedRoleName).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct); return roleId.HasValue ? await db.ApplicationUsers.Where(x => x.RoleId == roleId).ToListAsync(ct) : []; }
    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken ct) { user.SetSecurityStamp(stamp); return Task.CompletedTask; }
    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<string?>(user.SecurityStamp);
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.LockoutEnd);
    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? end, CancellationToken ct) { user.SetLockoutEnd(end); return Task.CompletedTask; }
    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) { user.SetAccessFailedCount(user.AccessFailedCount + 1); return Task.FromResult(user.AccessFailedCount); }
    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) { user.SetAccessFailedCount(0); return Task.CompletedTask; }
    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.AccessFailedCount);
    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.LockoutEnabled);
    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct) { user.SetLockoutEnabled(enabled); return Task.CompletedTask; }
    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct) { user.SetTwoFactorEnabled(enabled); return Task.CompletedTask; }
    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.TwoFactorEnabled);
}

public sealed class ProductAppRoleStore(ApplicationDbContext db) : IQueryableRoleStore<Role>
{
    public IQueryable<Role> Roles => db.Roles;
    public void Dispose() { }
    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken ct) { db.Roles.Add(role); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken ct) { role.SetConcurrencyStamp(Guid.NewGuid().ToString("N")); db.Roles.Update(role); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken ct) { db.Roles.Remove(role); await db.SaveChangesAsync(ct); return IdentityResult.Success; }
    public Task<string> GetRoleIdAsync(Role role, CancellationToken ct) => Task.FromResult(role.Id.ToString());
    public Task<string?> GetRoleNameAsync(Role role, CancellationToken ct) => Task.FromResult<string?>(role.Name);
    public Task SetRoleNameAsync(Role role, string? name, CancellationToken ct) { role.SetName(name); return Task.CompletedTask; }
    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken ct) => Task.FromResult<string?>(role.NormalizedName);
    public Task SetNormalizedRoleNameAsync(Role role, string? name, CancellationToken ct) { role.SetNormalizedName(name); return Task.CompletedTask; }
    public Task<Role?> FindByIdAsync(string id, CancellationToken ct) => Guid.TryParse(id, out var key) ? db.Roles.FindAsync([key], ct).AsTask() : Task.FromResult<Role?>(null);
    public Task<Role?> FindByNameAsync(string normalizedName, CancellationToken ct) => db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == normalizedName, ct);
}
