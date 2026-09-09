namespace ProductApp.Domain.Commercial;

public sealed class RiskScoreCalculator
{
    public decimal Calculate(IEnumerable<Risk> source)
    {
        var risks = source.ToArray();
        if (risks.Length == 0) return 100m;
        var averageSeverity = risks.Average(risk => (decimal)risk.Severity);
        var maximumSeverity = risks.Max(risk => risk.Severity);
        var exposure = (averageSeverity * .60m + maximumSeverity * .40m) / 25m * 100m;
        return decimal.Round(decimal.Clamp(100m - exposure, 0m, 100m), 2);
    }
}
