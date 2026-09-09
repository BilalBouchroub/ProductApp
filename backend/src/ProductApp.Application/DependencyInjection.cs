using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductApp.Application.Common.Behaviors;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Commercial;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        var assembly = typeof(DependencyInjection).Assembly;
        foreach (var implementation in assembly.DefinedTypes.Where(type => !type.IsAbstract && !type.IsInterface))
        {
            foreach (var contract in implementation.ImplementedInterfaces.Where(type =>
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>)))
            {
                services.AddTransient(contract, implementation);
            }
        }
        services.AddSingleton<ProductionCostCalculator>();
        services.AddSingleton<MarginCalculator>();
        services.AddSingleton<FeasibilityScoreCalculator>();
        services.AddSingleton<ProductApp.Domain.MarketAnalysis.RecommendationEngine>();
        services.AddSingleton<ProductionScoreCalculator>();
        services.AddSingleton<MarketScoreCalculator>();
        services.AddSingleton<FinancialScoreCalculator>();
        services.AddSingleton<RiskScoreCalculator>();
        services.AddSingleton<GlobalScoreCalculator>();
        services.AddSingleton<ProductApp.Domain.Commercial.RecommendationEngine>();
        services.AddScoped<ISmartProductService, SmartProductService>();

        return services;
    }
}
