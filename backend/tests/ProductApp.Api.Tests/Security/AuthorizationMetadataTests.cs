using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using ProductApp.Api.Controllers;

namespace ProductApp.Api.Tests.Security;

public sealed class AuthorizationMetadataTests
{
    [Theory]
    [InlineData(typeof(ProductionExperimentsController), "ProductionAccess")]
    [InlineData(typeof(MarketStudiesController), "CommercialAccess")]
    [InlineData(typeof(CommercialRisksController), "CommercialAccess")]
    public void Sensitive_controllers_require_the_expected_policy(Type controller, string policy)
    {
        var authorization = controller.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>();
        Assert.Contains(authorization, attribute => attribute.Policy == policy);
    }

    [Fact]
    public void Production_steps_are_readable_by_authenticated_users_but_writes_require_production_access()
    {
        var controllerAuthorization = typeof(ProductionStepsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();
        Assert.Contains(controllerAuthorization, attribute => attribute.Policy is null);

        var writeActions = new[]
        {
            nameof(ProductionStepsController.Create),
            nameof(ProductionStepsController.Update),
            nameof(ProductionStepsController.Delete),
            nameof(ProductionStepsController.Reorder),
        };
        foreach (var action in writeActions)
        {
            var authorization = typeof(ProductionStepsController).GetMethod(action)!
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>();
            Assert.Contains(authorization, attribute => attribute.Policy == "ProductionAccess");
        }
    }

    [Fact]
    public void Login_and_refresh_are_anonymous()
    {
        var methods = new[] { nameof(AuthController.Login), nameof(AuthController.Refresh), nameof(AuthController.ForgotPassword), nameof(AuthController.ResetPassword) };
        foreach (var name in methods)
            Assert.NotEmpty(typeof(AuthController).GetMethod(name)!.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }

    [Fact]
    public void Anonymous_authentication_endpoints_are_rate_limited()
    {
        var methods = new[] { nameof(AuthController.Login), nameof(AuthController.Refresh), nameof(AuthController.ForgotPassword), nameof(AuthController.ResetPassword) };
        foreach (var name in methods)
            Assert.NotEmpty(typeof(AuthController).GetMethod(name)!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true));
    }
}
