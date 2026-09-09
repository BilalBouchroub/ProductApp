namespace ProductApp.Application.SapIntegration.DTOs;

public sealed record SapProductionOrderDto(
    string OrderNumber,
    string ProductCode,
    string ProductDesignation,
    decimal PlannedQuantity,
    string Unit,
    DateTime PlannedStartUtc,
    DateTime PlannedEndUtc,
    string Status);
