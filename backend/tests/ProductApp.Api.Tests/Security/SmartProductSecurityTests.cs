using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using ProductApp.Api.Controllers;

namespace ProductApp.Api.Tests.Security;

public sealed class SmartProductSecurityTests
{
    [Fact]
    public void Smart_product_requires_authentication_and_rate_limiting()
    {
        Assert.NotEmpty(typeof(AiConversationsController).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        var limiter = Assert.Single(typeof(AiConversationsController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), true));
        Assert.Equal("smart-product", ((EnableRateLimitingAttribute)limiter).PolicyName);
    }

    [Fact]
    public void Smart_product_exposes_no_raw_sql_endpoint()
    {
        var actions = typeof(AiConversationsController).GetMethods().Where(method => method.DeclaringType == typeof(AiConversationsController));
        Assert.DoesNotContain(actions, action => action.Name.Contains("Sql", StringComparison.OrdinalIgnoreCase));
        Assert.All(actions, action => Assert.DoesNotContain(action.GetParameters(), parameter =>
            parameter.Name?.Contains("sql", StringComparison.OrdinalIgnoreCase) == true));
    }
}
