namespace ProductApp.Domain.MarketAnalysis;

public sealed class MarketAnalysis
{
    private MarketAnalysis()
    {
    }

    private MarketAnalysis(
        Guid productId,
        decimal materialCost,
        decimal laborCost,
        decimal equipmentCost,
        decimal totalProductionCost,
        decimal targetSellingPrice,
        decimal grossMargin,
        decimal marginRate,
        int totalCycleTimeMinutes,
        decimal materialAvailabilityRate,
        decimal feasibilityScore,
        MarketRecommendation recommendation,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        MaterialCost = materialCost;
        LaborCost = laborCost;
        EquipmentCost = equipmentCost;
        TotalProductionCost = totalProductionCost;
        TargetSellingPrice = targetSellingPrice;
        GrossMargin = grossMargin;
        MarginRate = marginRate;
        TotalCycleTimeMinutes = totalCycleTimeMinutes;
        MaterialAvailabilityRate = materialAvailabilityRate;
        FeasibilityScore = feasibilityScore;
        Recommendation = recommendation;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal MaterialCost { get; private set; }
    public decimal LaborCost { get; private set; }
    public decimal EquipmentCost { get; private set; }
    public decimal TotalProductionCost { get; private set; }
    public decimal TargetSellingPrice { get; private set; }
    public decimal GrossMargin { get; private set; }
    public decimal MarginRate { get; private set; }
    public int TotalCycleTimeMinutes { get; private set; }
    public decimal MaterialAvailabilityRate { get; private set; }
    public decimal FeasibilityScore { get; private set; }
    public MarketRecommendation Recommendation { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static MarketAnalysis Create(
        Guid productId,
        ProductionCostResult cost,
        decimal targetSellingPrice,
        MarginResult margin,
        int totalCycleTimeMinutes,
        decimal materialAvailabilityRate,
        decimal feasibilityScore,
        MarketRecommendation recommendation,
        DateTime createdAtUtc)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product identifier cannot be empty.", nameof(productId));
        }

        if (feasibilityScore is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(feasibilityScore));
        }

        return new MarketAnalysis(
            productId,
            cost.MaterialCost,
            cost.LaborCost,
            cost.EquipmentCost,
            cost.TotalProductionCost,
            targetSellingPrice,
            margin.GrossMargin,
            margin.MarginRate,
            totalCycleTimeMinutes,
            materialAvailabilityRate,
            feasibilityScore,
            recommendation,
            createdAtUtc);
    }
}
