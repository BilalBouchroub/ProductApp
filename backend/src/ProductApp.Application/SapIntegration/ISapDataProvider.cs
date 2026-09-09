using ProductApp.Application.SapIntegration.DTOs;

namespace ProductApp.Application.SapIntegration;

public interface ISapDataProvider
{
    Task<IReadOnlyList<SapMaterialDto>> GetMaterialsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SapBomDto>> GetBomsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SapProductionOrderDto>> GetProductionOrdersAsync(
        CancellationToken cancellationToken);
}
