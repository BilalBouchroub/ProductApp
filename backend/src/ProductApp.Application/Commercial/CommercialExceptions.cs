namespace ProductApp.Application.Commercial;

public sealed class MarketStudyNotFoundException(Guid id) : Exception($"Étude de marché '{id}' introuvable.");
public sealed class CompetitorNotFoundException(Guid id) : Exception($"Concurrent '{id}' introuvable.");
public sealed class CommercialRiskNotFoundException(Guid id) : Exception($"Risque '{id}' introuvable.");
public sealed class CommercialWorkflowException(string message) : Exception(message);
