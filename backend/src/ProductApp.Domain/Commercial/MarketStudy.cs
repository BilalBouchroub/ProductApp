using ProductApp.Domain.Common;
using ProductApp.Domain.MarketAnalysis;
using ProductApp.Domain.Products;

namespace ProductApp.Domain.Commercial;

public sealed class MarketStudy : AuditableEntity
{
    private MarketStudy() { }
    public Guid Id { get; private set; }
    public Guid ProductVersionId { get; private set; }
    public ProductVersion ProductVersion { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string TargetMarket { get; private set; } = string.Empty;
    public string GeographicArea { get; private set; } = string.Empty;
    public string CustomerSegment { get; private set; } = string.Empty;
    public DateTime StudyDate { get; private set; }
    public MarketStudyStatus Status { get; private set; }
    public decimal EstimatedMarketSize { get; private set; }
    public decimal AnnualGrowthRate { get; private set; }
    public decimal ProductionCost { get; private set; }
    public decimal ProposedSalePrice { get; private set; }
    public decimal AverageMarketPrice { get; private set; }
    public decimal CalculatedMargin { get; private set; }
    public decimal MarginRate { get; private set; }
    public decimal MonthlySalesVolume { get; private set; }
    public decimal AnnualRevenue { get; private set; }
    public decimal ProductionScore { get; private set; }
    public decimal MarketScore { get; private set; }
    public decimal FinancialScore { get; private set; }
    public decimal RiskScore { get; private set; }
    public decimal GlobalScore { get; private set; }
    public MarketRecommendation? Recommendation { get; private set; }
    public ICollection<Competitor> Competitors { get; private set; } = new List<Competitor>();
    public ICollection<Risk> Risks { get; private set; } = new List<Risk>();
    public ICollection<OptimizationRequest> OptimizationRequests { get; private set; } = new List<OptimizationRequest>();

    public static MarketStudy Create(ProductVersion productVersion, string name, string targetMarket,
        string geographicArea, string customerSegment, DateTime studyDate,
        decimal estimatedMarketSize, decimal annualGrowthRate, decimal productionCost,
        decimal proposedSalePrice, decimal averageMarketPrice, decimal monthlySalesVolume,
        DateTime? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(productVersion);
        var study = new MarketStudy { Id = Guid.NewGuid(), ProductVersionId = productVersion.Id, ProductVersion = productVersion };
        study.Apply(name, targetMarket, geographicArea, customerSegment, studyDate,
            estimatedMarketSize, annualGrowthRate, productionCost, proposedSalePrice,
            averageMarketPrice, monthlySalesVolume);
        study.Status = MarketStudyStatus.Draft;
        study.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return study;
    }

    public void Update(string name, string targetMarket, string geographicArea,
        string customerSegment, DateTime studyDate, decimal estimatedMarketSize,
        decimal annualGrowthRate, decimal productionCost, decimal proposedSalePrice,
        decimal averageMarketPrice, decimal monthlySalesVolume, DateTime updatedAt)
    {
        EnsureEditable();
        Apply(name, targetMarket, geographicArea, customerSegment, studyDate,
            estimatedMarketSize, annualGrowthRate, productionCost, proposedSalePrice,
            averageMarketPrice, monthlySalesVolume);
        Status = MarketStudyStatus.InProgress;
        Touch(updatedAt);
    }

    public void SaveAsDraft(string name, string targetMarket, string geographicArea,
        string customerSegment, DateTime studyDate, decimal estimatedMarketSize,
        decimal annualGrowthRate, decimal productionCost, decimal proposedSalePrice,
        decimal averageMarketPrice, decimal monthlySalesVolume, DateTime updatedAt)
    {
        EnsureEditable();
        Apply(name, targetMarket, geographicArea, customerSegment, studyDate,
            estimatedMarketSize, annualGrowthRate, productionCost, proposedSalePrice,
            averageMarketPrice, monthlySalesVolume);
        Status = MarketStudyStatus.Draft;
        Touch(updatedAt);
    }

    public void Validate(CommercialScoreResult scores, MarketRecommendation recommendation,
        DateTime validatedAt)
    {
        EnsureEditable();
        ProductionScore = scores.ProductionScore;
        MarketScore = scores.MarketScore;
        FinancialScore = scores.FinancialScore;
        RiskScore = scores.RiskScore;
        GlobalScore = scores.GlobalScore;
        Recommendation = recommendation;
        Status = MarketStudyStatus.Validated;
        Touch(validatedAt);
    }

    public bool CanBeDeleted => Status == MarketStudyStatus.Draft;

    private void Apply(string name, string targetMarket, string geographicArea,
        string customerSegment, DateTime studyDate, decimal estimatedMarketSize,
        decimal annualGrowthRate, decimal productionCost, decimal proposedSalePrice,
        decimal averageMarketPrice, decimal monthlySalesVolume)
    {
        if (estimatedMarketSize < 0 || productionCost < 0 || proposedSalePrice <= 0
            || averageMarketPrice < 0 || monthlySalesVolume < 0)
            throw new ArgumentOutOfRangeException(nameof(proposedSalePrice));
        Name = name.Trim(); TargetMarket = targetMarket.Trim(); GeographicArea = geographicArea.Trim();
        CustomerSegment = customerSegment.Trim(); StudyDate = studyDate;
        EstimatedMarketSize = estimatedMarketSize; AnnualGrowthRate = annualGrowthRate;
        ProductionCost = productionCost; ProposedSalePrice = proposedSalePrice;
        AverageMarketPrice = averageMarketPrice; MonthlySalesVolume = monthlySalesVolume;
        CalculatedMargin = decimal.Round(proposedSalePrice - productionCost, 2);
        MarginRate = productionCost == 0 ? 100m : decimal.Round(CalculatedMargin / productionCost * 100m, 2);
        AnnualRevenue = decimal.Round(monthlySalesVolume * proposedSalePrice * 12m, 2);
    }

    private void EnsureEditable()
    {
        if (Status == MarketStudyStatus.Validated)
            throw new InvalidOperationException("Une étude validée ne peut plus être modifiée.");
    }
}
