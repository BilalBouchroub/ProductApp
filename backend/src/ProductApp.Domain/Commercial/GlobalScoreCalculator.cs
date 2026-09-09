namespace ProductApp.Domain.Commercial;

public sealed class GlobalScoreCalculator
{
    public CommercialScoreResult Calculate(decimal productionScore, decimal marketScore,
        decimal financialScore, decimal riskScore)
    {
        Validate(productionScore); Validate(marketScore); Validate(financialScore); Validate(riskScore);
        var global = productionScore * .30m + marketScore * .25m
            + financialScore * .25m + riskScore * .20m;
        return new CommercialScoreResult(productionScore, marketScore, financialScore,
            riskScore, decimal.Round(global, 2));
    }

    private static void Validate(decimal score)
    {
        if (score is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(score));
    }
}
