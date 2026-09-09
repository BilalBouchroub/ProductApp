using Microsoft.EntityFrameworkCore;
using ProductApp.Application.Production.References;
using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Production;

public sealed class ProductionReferenceRepository(ApplicationDbContext dbContext)
    : IProductionReferenceRepository
{
    public async Task<IReadOnlyList<ProductCategoryOptionDto>> ListProductCategoriesAsync(
        CancellationToken cancellationToken) =>
        await dbContext.ProductCategories.AsNoTracking().Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => new ProductCategoryOptionDto(item.Id, item.Code, item.Name, item.Description))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EquipmentOptionDto>> ListEquipmentAsync(
        CancellationToken cancellationToken)
    {
        var equipment = await dbContext.Equipment.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Code, item.Name, item.AvailabilityStatus, item.HourlyCost })
            .ToListAsync(cancellationToken);
        return equipment.Select(item => new EquipmentOptionDto(item.Id, item.Code, item.Name,
            item.AvailabilityStatus.ToString(), item.HourlyCost)).ToArray();
    }
}
