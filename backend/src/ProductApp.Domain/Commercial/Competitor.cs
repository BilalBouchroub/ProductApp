using ProductApp.Domain.Common;

namespace ProductApp.Domain.Commercial;

public sealed class Competitor : AuditableEntity
{
    private Competitor() { }
    public Guid Id { get; private set; }
    public Guid MarketStudyId { get; private set; }
    public MarketStudy MarketStudy { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? EstimatedQualityScore { get; private set; }
    public decimal? MarketShare { get; private set; }
    public string? Strengths { get; private set; }
    public string? Weaknesses { get; private set; }
    public string? SalesChannels { get; private set; }
    public decimal? CustomerRating { get; private set; }

    public static Competitor Create(Guid studyId, string name, string productName,
        decimal price, decimal quantity, decimal? qualityScore, decimal? marketShare,
        string? strengths, string? weaknesses, string? salesChannels,
        decimal? customerRating, DateTime? createdAt = null)
    {
        var competitor = new Competitor { Id = Guid.NewGuid(), MarketStudyId = studyId };
        competitor.Apply(name, productName, price, quantity, qualityScore, marketShare,
            strengths, weaknesses, salesChannels, customerRating);
        competitor.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return competitor;
    }

    public void Update(string name, string productName, decimal price, decimal quantity,
        decimal? qualityScore, decimal? marketShare, string? strengths, string? weaknesses,
        string? salesChannels, decimal? customerRating, DateTime updatedAt)
    {
        Apply(name, productName, price, quantity, qualityScore, marketShare,
            strengths, weaknesses, salesChannels, customerRating);
        Touch(updatedAt);
    }

    private void Apply(string name, string productName, decimal price, decimal quantity,
        decimal? qualityScore, decimal? marketShare, string? strengths, string? weaknesses,
        string? salesChannels, decimal? customerRating)
    {
        if (price < 0 || quantity <= 0 || qualityScore is < 0 or > 100
            || marketShare is < 0 or > 100 || customerRating is < 0 or > 5)
            throw new ArgumentOutOfRangeException(nameof(price));
        Name = name.Trim(); ProductName = productName.Trim(); Price = price; Quantity = quantity;
        EstimatedQualityScore = qualityScore; MarketShare = marketShare;
        Strengths = strengths?.Trim(); Weaknesses = weaknesses?.Trim();
        SalesChannels = salesChannels?.Trim(); CustomerRating = customerRating;
    }
}
