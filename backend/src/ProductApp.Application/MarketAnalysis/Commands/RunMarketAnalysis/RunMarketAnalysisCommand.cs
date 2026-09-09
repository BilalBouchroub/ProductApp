using FluentValidation;
using MediatR;
using ProductApp.Application.MarketAnalysis.DTOs;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps;
using ProductApp.Application.SapIntegration;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Products;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Application.MarketAnalysis.Commands.RunMarketAnalysis;

public sealed record MaterialRequirement(string SapCode, decimal Quantity);

public sealed record RunMarketAnalysisCommand(
    Guid ProductId,
    decimal TargetSellingPrice,
    decimal EquipmentCost,
    IReadOnlyList<MaterialRequirement> Materials) : IRequest<MarketAnalysisDto>;

public sealed class RunMarketAnalysisCommandValidator : AbstractValidator<RunMarketAnalysisCommand>
{
    public RunMarketAnalysisCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.TargetSellingPrice).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(command => command.EquipmentCost).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(command => command.Materials).NotEmpty();
        RuleFor(command => command.Materials)
            .Must(materials => materials.Select(item => item.SapCode)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == materials.Count)
            .WithMessage("SAP material codes must be unique.");
        RuleForEach(command => command.Materials).ChildRules(material =>
        {
            material.RuleFor(item => item.SapCode).NotEmpty().MaximumLength(50);
            material.RuleFor(item => item.Quantity).GreaterThan(0).PrecisionScale(18, 3, true);
        });
    }
}

public sealed class RunMarketAnalysisCommandHandler(
    IMarketAnalysisRepository analysisRepository,
    IProductRepository productRepository,
    IProductionStepRepository productionStepRepository,
    ISapDataProvider sapDataProvider,
    ProductionCostCalculator costCalculator,
    MarginCalculator marginCalculator,
    FeasibilityScoreCalculator scoreCalculator,
    RecommendationEngine recommendationEngine)
    : IRequestHandler<RunMarketAnalysisCommand, MarketAnalysisDto>
{
    public async Task<MarketAnalysisDto> Handle(
        RunMarketAnalysisCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);
        if (product.Status == ProductStatus.Archived)
        {
            throw new ProductArchivedException(product.Id);
        }

        var productionSteps = await productionStepRepository.ListAsync(
            request.ProductId, cancellationToken);
        if (productionSteps.Count == 0)
        {
            throw new MarketAnalysisPrerequisiteException(
                "At least one production step is required to run a market analysis.");
        }

        var sapMaterials = await sapDataProvider.GetMaterialsAsync(cancellationToken);
        var sapMaterialsByCode = sapMaterials.ToDictionary(
            material => material.SapCode,
            StringComparer.OrdinalIgnoreCase);
        var costLines = new List<MaterialCostLine>(request.Materials.Count);
        var availabilityRates = new List<decimal>(request.Materials.Count);

        foreach (var requirement in request.Materials)
        {
            if (!sapMaterialsByCode.TryGetValue(requirement.SapCode, out var material))
            {
                throw new SapMaterialNotFoundException(requirement.SapCode);
            }

            costLines.Add(new MaterialCostLine(material.UnitCost, requirement.Quantity));
            availabilityRates.Add(Math.Min(material.AvailableQuantity / requirement.Quantity, 1m));
        }

        var laborCost = productionSteps.Sum(step => step.LaborCost);
        var totalCycleTimeMinutes = productionSteps.Sum(step => step.DurationMinutes);
        var materialAvailabilityRate = availabilityRates.Min();
        var cost = costCalculator.Calculate(costLines, laborCost, request.EquipmentCost);
        var margin = marginCalculator.Calculate(request.TargetSellingPrice, cost.TotalProductionCost);
        var score = scoreCalculator.Calculate(
            margin.MarginRate, materialAvailabilityRate, totalCycleTimeMinutes);
        var recommendation = recommendationEngine.GetRecommendation(score);
        var analysis = MarketAnalysisEntity.Create(
            request.ProductId,
            cost,
            request.TargetSellingPrice,
            margin,
            totalCycleTimeMinutes,
            materialAvailabilityRate,
            score,
            recommendation,
            DateTime.UtcNow);

        await analysisRepository.AddAsync(analysis, cancellationToken);
        await analysisRepository.SaveChangesAsync(cancellationToken);
        return analysis.ToDto();
    }
}
