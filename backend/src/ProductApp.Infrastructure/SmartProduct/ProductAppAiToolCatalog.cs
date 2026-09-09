using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Auth;
using ProductApp.Application.SmartProduct;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.SmartProduct;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed partial class ProductAppAiToolCatalog(ApplicationDbContext db) : IAiBusinessToolCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiBusinessContext> BuildAuthorizedContextAsync(AiUserContext user, Guid? productId,
        Guid? experimentId, string prompt, CancellationToken ct)
    {
        var results = new List<AiToolResult>
        {
            Result("get_current_user_context", new { user.UserId, user.DisplayName, user.Role, mode = "read_only" },
                [new AiToolSource(AiSourceKind.UserContext, user.UserId, user.DisplayName)])
        };

        var products = await SearchProductsAsync(productId, prompt, ct);
        if (products.Count > 0)
        {
            results.Add(Result("search_products", products.Select(p => new
            {
                p.Id, p.Code, p.Name, p.Description, p.Status, p.VersionNumber, p.SapCode,
                category = p.ProductCategory == null ? null : p.ProductCategory.Name
            }), products.Select(p => ProductSource(p.Id, p.Code, p.Name, user.Role)).ToArray()));
        }

        var selectedProduct = productId.HasValue
            ? products.FirstOrDefault(p => p.Id == productId.Value)
            : products.Count == 1 ? products[0] : null;
        if (selectedProduct is not null)
        {
            var version = await db.ProductVersions.AsNoTracking().AsSplitQuery()
                .Include(x => x.ProductionSteps).ThenInclude(x => x.Resources)
                .Include(x => x.Experiments).ThenInclude(x => x.Steps)
                .SingleOrDefaultAsync(x => x.ProductId == selectedProduct.Id && x.VersionNumber == selectedProduct.VersionNumber, ct);
            if (version is not null)
            {
                var chain = ProductionChainCalculator.Calculate(version.ProductionSteps);
                var steps = version.ProductionSteps.OrderBy(x => x.Order).Select(step => new
                {
                    step.Id, step.Order, step.Name, step.Description, step.DurationMinutes, step.ActualDurationMinutes,
                    step.PlannedCost, step.ActualCost, step.LaborCost, step.EquipmentName, step.EnergyConsumption,
                    step.WasteQuantity, step.PlannedOutputQuantity, step.ActualOutputQuantity, step.Status,
                    resources = step.Resources.Select(resource => new
                    {
                        resource.Id, resource.Designation, resource.ResourceType, resource.Unit,
                        resource.PlannedQuantity, resource.ActualQuantity, resource.UnitCost,
                        resource.TotalCost, resource.AvailableStock, resource.AvailabilityStatus
                    })
                }).ToArray();
                results.Add(Result("get_product_production_analysis", new
                {
                    product = new { selectedProduct.Id, selectedProduct.Code, selectedProduct.Name, selectedProduct.Description,
                        selectedProduct.TargetSalePrice, selectedProduct.BatchQuantity, selectedProduct.ProductionUnit },
                    version = new { version.Id, version.VersionNumber, version.Status },
                    calculated = new { chain.TotalDurationMinutes, chain.TotalCost, chain.ResourceCount, chain.MachineCount, chain.EnergyConsumption,
                        criticalCostStep = steps.OrderByDescending(x => x.ActualCost ?? x.PlannedCost ?? x.LaborCost).FirstOrDefault()?.Name,
                        criticalDurationStep = steps.OrderByDescending(x => x.ActualDurationMinutes ?? x.DurationMinutes).FirstOrDefault()?.Name },
                    steps
                }, [ProductSource(selectedProduct.Id, selectedProduct.Code, selectedProduct.Name, user.Role),
                    .. version.ProductionSteps.Select(step => new AiToolSource(AiSourceKind.ProductionStep, step.Id, step.Name,
                        $"Étape {step.Order}", ProductUrl(selectedProduct.Id, user.Role))) ]));

                if (version.Experiments.Count > 0)
                    results.Add(Result("get_product_experiments", version.Experiments.OrderByDescending(x => x.StartDate).Take(20).Select(ExperimentView),
                        version.Experiments.Select(exp => new AiToolSource(AiSourceKind.Experiment, exp.Id, exp.Name,
                            exp.Result.ToString(), ExperimentUrl(exp.Id, user.Role))).ToArray()));
            }
        }

        var namedExperiments = ExperimentReferenceRegex().Matches(prompt).Select(match => match.Value).Distinct(StringComparer.OrdinalIgnoreCase).Take(5).ToArray();
        if (experimentId.HasValue || namedExperiments.Length > 0)
        {
            var experiments = await db.ProductionExperiments.AsNoTracking().Include(x => x.Steps)
                .Include(x => x.ProductVersion).ThenInclude(x => x.Product)
                .Where(x => (experimentId.HasValue && x.Id == experimentId.Value) || namedExperiments.Contains(x.Name))
                .Take(10).ToListAsync(ct);
            if (experiments.Count > 0)
                results.Add(Result(experiments.Count > 1 ? "compare_experiments" : "get_experiment", experiments.Select(ExperimentView),
                    experiments.Select(exp => new AiToolSource(AiSourceKind.Experiment, exp.Id, exp.Name,
                        exp.Result.ToString(), ExperimentUrl(exp.Id, user.Role))).ToArray()));
        }

        return new AiBusinessContext(results);
    }

    private async Task<List<ProductApp.Domain.Products.Product>> SearchProductsAsync(Guid? productId, string prompt, CancellationToken ct)
    {
        var references = ProductReferenceRegex().Matches(prompt).Select(match => match.Value).Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToArray();
        if (!productId.HasValue && references.Length == 0 && IsProductCatalogRequest(prompt))
        {
            return await db.Products.AsNoTracking().Include(x => x.ProductCategory)
                .Where(x => x.Status == ProductApp.Domain.Products.ProductStatus.Active)
                .OrderBy(x => x.Code).Take(50).ToListAsync(ct);
        }

        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length >= 3).Take(8).ToArray();
        return await db.Products.AsNoTracking().Include(x => x.ProductCategory)
            .Where(x => (productId.HasValue && x.Id == productId.Value)
                || references.Contains(x.Code)
                || words.Any(word => x.Name.Contains(word)))
            .OrderBy(x => x.Code).Take(8).ToListAsync(ct);
    }

    internal static bool IsProductCatalogRequest(string prompt) =>
        ProductCatalogSubjectRegex().IsMatch(prompt)
        && ProductCatalogActionRegex().IsMatch(prompt);

    private static object ExperimentView(ProductApp.Domain.Experiments.ProductionExperiment exp) => new
    {
        exp.Id, exp.Name, exp.Objective, exp.Hypothesis, exp.StartDate, exp.EndDate,
        exp.PlannedQuantity, exp.ActualQuantity, exp.PlannedCost, exp.ActualCost,
        exp.PlannedDurationMinutes, exp.ActualDurationMinutes, exp.WasteRate, exp.Result,
        exp.Observations, exp.Conclusion,
        costVariance = exp.ActualCost.HasValue ? exp.ActualCost.Value - exp.PlannedCost : (decimal?)null,
        durationVarianceMinutes = exp.ActualDurationMinutes.HasValue ? exp.ActualDurationMinutes.Value - exp.PlannedDurationMinutes : (int?)null,
        steps = exp.Steps.OrderBy(x => x.Order).Select(x => new { x.ProductionStepId, x.Order,
            x.PlannedCost, x.ActualCost, x.PlannedDurationMinutes, x.ActualDurationMinutes,
            x.PlannedOutputQuantity, x.ActualOutputQuantity, x.IsValidated })
    };

    private static AiToolResult Result(string name, object value, IReadOnlyList<AiToolSource> sources) =>
        new(name, JsonSerializer.Serialize(value, JsonOptions), sources);
    private static AiToolSource ProductSource(Guid id, string code, string name, string role) =>
        new(AiSourceKind.Product, id, $"{code} — {name}", code, ProductUrl(id, role));
    private static string? ProductUrl(Guid id, string role) => role switch
    {
        AppRoles.CommercialManager => $"/commercial/products/{id}",
        AppRoles.ProductionManager => $"/production/products/{id}",
        _ => null
    };
    private static string? ExperimentUrl(Guid id, string role) => role switch
    {
        AppRoles.CommercialManager => "/commercial/products",
        AppRoles.ProductionManager => $"/production/experiments/{id}",
        _ => null
    };

    [GeneratedRegex(@"\b[A-Za-z]{1,12}[-_]?[0-9]{1,12}\b", RegexOptions.IgnoreCase)]
    private static partial Regex ProductReferenceRegex();
    [GeneratedRegex(@"\bEXP[-_ ]?[0-9]{1,12}\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExperimentReferenceRegex();
    [GeneratedRegex(@"\b(produits?|catalogue)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ProductCatalogSubjectRegex();
    [GeneratedRegex(@"\b(liste|lister|affiche|afficher|montre|montrer|voir|quels?|tous|existants?|disponibles?|combien)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ProductCatalogActionRegex();
}
