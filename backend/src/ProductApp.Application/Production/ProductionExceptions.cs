namespace ProductApp.Application.Production;

public sealed class ProductVersionNotFoundException(Guid productId, int version)
    : Exception($"Version {version} du produit '{productId}' introuvable.");
public sealed class ProductionWorkflowException(string message) : Exception(message);
public sealed class StepResourceNotFoundException(Guid id) : Exception($"Ressource d’étape '{id}' introuvable.");
public sealed class ProductionExperimentNotFoundException(Guid id) : Exception($"Expérience '{id}' introuvable.");
