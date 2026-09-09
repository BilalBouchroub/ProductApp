using MediatR;
using ProductApp.Application.Products.DTOs;

namespace ProductApp.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(bool IncludeArchived = false) : IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetProductsQueryHandler(IProductRepository repository)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(request.IncludeArchived, cancellationToken);
        return products.Select(product => product.ToDto()).ToArray();
    }
}
