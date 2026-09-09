using ProductApp.Domain.Products;

namespace ProductApp.Domain.ProductionSteps;

public sealed record ProductionReadinessResult(bool IsReady, IReadOnlyList<string> Errors);

public static class ProductionReadinessPolicy
{
    public static ProductionReadinessResult Evaluate(ProductVersion version)
    {
        var errors = new List<string>();
        if (version.ProductionSteps.Count == 0) errors.Add("La chaîne de production est absente.");
        if (version.Experiments.Count == 0) errors.Add("Au moins une expérience est requise.");
        if (version.ProductionSteps.Any(step => !step.IsValid)) errors.Add("La chaîne contient une étape invalide.");
        if (version.ProductionSteps.Any(step => step.LaborCost < 0 || step.Resources.Any(resource => resource.UnitCost < 0)))
            errors.Add("Tous les coûts doivent être renseignés.");

        try { _ = ProductionChainCalculator.Calculate(version.ProductionSteps); }
        catch (InvalidOperationException exception) { errors.Add(exception.Message); }
        return new ProductionReadinessResult(errors.Count == 0, errors);
    }
}
