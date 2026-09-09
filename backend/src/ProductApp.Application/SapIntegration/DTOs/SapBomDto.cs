namespace ProductApp.Application.SapIntegration.DTOs;

public sealed record SapBomDto(
    string ProductCode,
    string ProductDesignation,
    decimal BaseQuantity,
    string BaseUnit,
    IReadOnlyList<SapBomComponentDto> Components);

public sealed record SapBomComponentDto(
    string MaterialCode,
    string MaterialDesignation,
    decimal RequiredQuantity,
    string Unit);
