using MediatR;

namespace ProductApp.Application.Production.References;

public sealed record ProductCategoryOptionDto(Guid Id, string Code, string Name, string? Description);
public sealed record EquipmentOptionDto(Guid Id, string Code, string Name, string AvailabilityStatus, decimal HourlyCost);

public interface IProductionReferenceRepository
{
    Task<IReadOnlyList<ProductCategoryOptionDto>> ListProductCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<EquipmentOptionDto>> ListEquipmentAsync(CancellationToken cancellationToken);
}

public sealed record GetProductCategoriesQuery : IRequest<IReadOnlyList<ProductCategoryOptionDto>>;
public sealed record GetEquipmentOptionsQuery : IRequest<IReadOnlyList<EquipmentOptionDto>>;

public sealed class GetProductCategoriesQueryHandler(IProductionReferenceRepository repository)
    : IRequestHandler<GetProductCategoriesQuery, IReadOnlyList<ProductCategoryOptionDto>>
{
    public Task<IReadOnlyList<ProductCategoryOptionDto>> Handle(GetProductCategoriesQuery request,
        CancellationToken cancellationToken) => repository.ListProductCategoriesAsync(cancellationToken);
}

public sealed class GetEquipmentOptionsQueryHandler(IProductionReferenceRepository repository)
    : IRequestHandler<GetEquipmentOptionsQuery, IReadOnlyList<EquipmentOptionDto>>
{
    public Task<IReadOnlyList<EquipmentOptionDto>> Handle(GetEquipmentOptionsQuery request,
        CancellationToken cancellationToken) => repository.ListEquipmentAsync(cancellationToken);
}
