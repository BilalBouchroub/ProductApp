namespace ProductApp.Domain.MarketAnalysis;

public sealed class RecommendationEngine
{
    public MarketRecommendation GetRecommendation(decimal feasibilityScore)
    {
        if (feasibilityScore is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(feasibilityScore));
        }

        return feasibilityScore switch
        {
            >= 70m => MarketRecommendation.Viable,
            >= 40m => MarketRecommendation.ToOptimize,
            _ => MarketRecommendation.NotViable
        };
    }
}
