using Eims.Application.Dtos;

namespace Eims.Application.Abstractions;

public interface IMasterDataService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<CategoryDto> CreateCategoryAsync(CategoryRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken);
    Task<ProductDto> CreateProductAsync(ProductRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(CancellationToken cancellationToken);
    Task<WarehouseDto> CreateWarehouseAsync(WarehouseRequest request, CancellationToken cancellationToken);
}
