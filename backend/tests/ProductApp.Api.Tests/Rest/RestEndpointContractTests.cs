using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ProductApp.Api.Tests.Rest;

public sealed class RestEndpointContractTests
{
    private static readonly Type[] RequiredControllers =
    [
        typeof(ProductApp.Api.Controllers.ProductsController), typeof(ProductApp.Api.Controllers.ProductionController),
        typeof(ProductApp.Api.Controllers.ExperimentsController), typeof(ProductApp.Api.Controllers.CommercialController),
        typeof(ProductApp.Api.Controllers.MarketStudiesController), typeof(ProductApp.Api.Controllers.NotificationsController),
        typeof(ProductApp.Api.Controllers.AuditController), typeof(ProductApp.Api.Controllers.UsersController),
        typeof(ProductApp.Api.Controllers.RolesController)
    ];

    [Fact]
    public void Every_required_REST_controller_is_an_api_controller_with_an_explicit_route()
    {
        foreach (var controller in RequiredControllers)
        {
            Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
            Assert.False(string.IsNullOrWhiteSpace(controller.GetCustomAttribute<RouteAttribute>()?.Template));
        }
    }

    [Fact]
    public void Every_public_controller_action_declares_an_HTTP_verb()
    {
        var controllers = typeof(ProductApp.Api.Controllers.ProductsController).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(ControllerBase).IsAssignableFrom(x));
        var actions = controllers.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(x => !x.IsSpecialName).ToArray();
        Assert.True(actions.Length >= 45);
        foreach (var action in actions)
            Assert.NotEmpty(action.GetCustomAttributes<HttpMethodAttribute>());
    }

    [Fact]
    public void REST_routes_and_verbs_are_unique()
    {
        var endpoints = typeof(ProductApp.Api.Controllers.ProductsController).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(ControllerBase).IsAssignableFrom(x))
            .SelectMany(controller =>
            {
                var prefix = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
                return controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .SelectMany(method => method.GetCustomAttributes<HttpMethodAttribute>()
                        .SelectMany(http => http.HttpMethods.Select(verb => $"{verb}:{prefix}/{http.Template}")));
            }).ToArray();
        Assert.Equal(endpoints.Length, endpoints.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Administrative_controllers_require_administrator_policy()
    {
        foreach (var controller in new[] { typeof(ProductApp.Api.Controllers.UsersController), typeof(ProductApp.Api.Controllers.RolesController), typeof(ProductApp.Api.Controllers.AuditController) })
            Assert.Contains(controller.GetCustomAttributes<AuthorizeAttribute>(), x => x.Policy == "AdministratorAccess");
    }

    [Fact]
    public void User_administration_exposes_one_explicit_action_endpoint()
    {
        var method = typeof(ProductApp.Api.Controllers.UsersController)
            .GetMethod(nameof(ProductApp.Api.Controllers.UsersController.ExecuteAction));
        var route = Assert.Single(method!.GetCustomAttributes<HttpPostAttribute>());

        Assert.Equal("{id:guid}/actions", route.Template);
        Assert.Null(typeof(ProductApp.Api.Controllers.UsersController).GetMethod("ChangeStatus"));
        Assert.Null(typeof(ProductApp.Api.Controllers.UsersController).GetMethod("ChangeRole"));
        Assert.Null(typeof(ProductApp.Api.Controllers.UsersController).GetMethod("Delete"));
    }
}
