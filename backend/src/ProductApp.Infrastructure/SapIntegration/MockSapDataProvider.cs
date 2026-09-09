using ProductApp.Application.SapIntegration;
using ProductApp.Application.SapIntegration.DTOs;

namespace ProductApp.Infrastructure.SapIntegration;

public sealed class MockSapDataProvider : ISapDataProvider
{
    private static readonly IReadOnlyList<SapMaterialDto> Materials =
    [
        new("MAT-FARINE-001", "Farine de blé", "KG", 0.85m, 12500m),
        new("MAT-SUCRE-001", "Sucre blanc", "KG", 1.10m, 8200m),
        new("MAT-BEURRE-001", "Beurre", "KG", 5.80m, 2100m),
        new("MAT-CHOCOLAT-001", "Chocolat noir", "KG", 7.25m, 1750m),
        new("MAT-EMBALLAGE-001", "Emballage biscuit", "UN", 0.12m, 50000m)
    ];

    private static readonly IReadOnlyList<SapBomDto> Boms =
    [
        new(
            "BISCUIT-CHOC-001",
            "Biscuit au chocolat",
            1m,
            "KG",
            [
                new("MAT-FARINE-001", "Farine de blé", 0.55m, "KG"),
                new("MAT-SUCRE-001", "Sucre blanc", 0.20m, "KG"),
                new("MAT-BEURRE-001", "Beurre", 0.15m, "KG"),
                new("MAT-CHOCOLAT-001", "Chocolat noir", 0.10m, "KG"),
                new("MAT-EMBALLAGE-001", "Emballage biscuit", 10m, "UN")
            ])
    ];

    private static readonly IReadOnlyList<SapProductionOrderDto> ProductionOrders =
    [
        new(
            "OF-2026-0001",
            "BISCUIT-CHOC-001",
            "Biscuit au chocolat",
            1000m,
            "KG",
            new DateTime(2026, 7, 29, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 29, 14, 0, 0, DateTimeKind.Utc),
            "Planned"),
        new(
            "OF-2026-0002",
            "BISCUIT-CHOC-001",
            "Biscuit au chocolat",
            750m,
            "KG",
            new DateTime(2026, 7, 30, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc),
            "Released")
    ];

    public Task<IReadOnlyList<SapMaterialDto>> GetMaterialsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Materials);
    }

    public Task<IReadOnlyList<SapBomDto>> GetBomsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Boms);
    }

    public Task<IReadOnlyList<SapProductionOrderDto>> GetProductionOrdersAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ProductionOrders);
    }
}
