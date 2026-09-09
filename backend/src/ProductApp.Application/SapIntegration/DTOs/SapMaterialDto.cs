namespace ProductApp.Application.SapIntegration.DTOs;

public sealed record SapMaterialDto(
    string SapCode,
    string Designation,
    string Unit,
    decimal UnitCost,
    decimal AvailableQuantity);
