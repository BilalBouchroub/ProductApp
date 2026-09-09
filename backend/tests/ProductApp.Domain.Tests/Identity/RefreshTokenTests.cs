using ProductApp.Domain.Identity;

namespace ProductApp.Domain.Tests.Identity;

public sealed class RefreshTokenTests
{
    [Fact]
    public void New_token_is_active_until_expiration()
    {
        var now = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        var token = RefreshToken.Create(Guid.NewGuid(), new string('A', 64), now.AddDays(7), "127.0.0.1", "tests", now);
        Assert.True(token.IsActive(now));
        Assert.False(token.IsActive(now.AddDays(8)));
    }

    [Fact]
    public void Revocation_is_idempotent_and_records_replacement()
    {
        var now = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        var token = RefreshToken.Create(Guid.NewGuid(), new string('A', 64), now.AddDays(7), "127.0.0.1", null, now);
        token.Revoke(now.AddMinutes(1), "10.0.0.1", new string('B', 64));
        token.Revoke(now.AddMinutes(2), "10.0.0.2");
        Assert.False(token.IsActive(now.AddMinutes(1)));
        Assert.Equal("10.0.0.1", token.RevokedByIp);
        Assert.Equal(new string('B', 64), token.ReplacedByTokenHash);
    }
}
