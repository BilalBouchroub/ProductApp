using ProductApp.Domain.Common;
using ProductApp.Domain.Identity;

namespace ProductApp.Domain.Tests.Identity;

public sealed class ApplicationUserTests
{
    [Fact]
    public void MarkDeleted_AnonymizesAndDisablesTheAccount()
    {
        var user = ApplicationUser.Create("Test", "User", "test.user@productapp.local", Guid.NewGuid(), DateTime.UtcNow);

        user.MarkDeleted(DateTime.UtcNow.AddMinutes(1));

        Assert.Equal(UserStatus.Deleted, user.Status);
        Assert.Equal("Utilisateur", user.FirstName);
        Assert.Equal("Supprime", user.LastName);
        Assert.EndsWith("@productapp.invalid", user.Email);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.RoleId);
        Assert.False(user.MustChangePassword);
        Assert.Equal("DELETED", user.PasswordHash);
    }
}
