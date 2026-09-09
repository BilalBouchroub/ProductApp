namespace ProductApp.Application.ProductionSteps;

public sealed class ProductionStepNotFoundException(Guid stepId)
    : Exception($"Production step '{stepId}' was not found.");

public sealed class ProductionStepOrderConflictException(Guid productId, int order)
    : Exception($"Product '{productId}' already has a production step at order {order}.");

public sealed class InvalidProductionStepOrderingException()
    : Exception("The ordered step identifiers must contain every production step exactly once.");
