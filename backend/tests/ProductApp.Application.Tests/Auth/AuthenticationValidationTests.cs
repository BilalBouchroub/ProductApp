using ProductApp.Application.Auth;

namespace ProductApp.Application.Tests.Auth;

public sealed class AuthenticationValidationTests
{
    [Fact]
    public void Login_rejects_invalid_email()
    {
        var result = new LoginCommandValidator().Validate(new LoginCommand("invalid", "Password!123", "127.0.0.1", null));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("alllowercase123!")]
    [InlineData("ALLUPPERCASE123!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar123")]
    public void Change_password_rejects_weak_passwords(string password)
    {
        var result = new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand(Guid.NewGuid(), "Current!12345", password));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Reset_password_accepts_a_strong_password()
    {
        var result = new ResetPasswordCommandValidator().Validate(new ResetPasswordCommand("user@example.com", "token", "StrongPassword!2026"));
        Assert.True(result.IsValid);
    }
}
