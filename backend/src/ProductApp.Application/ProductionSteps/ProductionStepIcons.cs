using ProductApp.Domain.ProductionSteps;

namespace ProductApp.Application.ProductionSteps;

public static class ProductionStepIcons
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        ProductionStepIconNames.Automatic,
        ProductionStepIconNames.Materials,
        ProductionStepIconNames.Preparation,
        ProductionStepIconNames.Mixing,
        ProductionStepIconNames.Processing,
        ProductionStepIconNames.Heating,
        ProductionStepIconNames.Cooling,
        ProductionStepIconNames.QualityControl,
        ProductionStepIconNames.Packaging,
        ProductionStepIconNames.Storage,
        ProductionStepIconNames.Palletizing
    };
}
