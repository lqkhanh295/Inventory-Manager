using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eims.Api.Controllers;

[ApiController]
[Route("api/master-data")]
[Authorize]
public sealed class MasterDataController(IMasterDataService masterData) : ControllerBase
{
    [HttpGet("categories")]
    public Task<IReadOnlyList<CategoryDto>> GetCategories(CancellationToken cancellationToken)
        => masterData.GetCategoriesAsync(cancellationToken);

    [HttpPost("categories")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public Task<CategoryDto> CreateCategory(CategoryRequest request, CancellationToken cancellationToken)
        => masterData.CreateCategoryAsync(request, cancellationToken);

    [HttpGet("products")]
    public Task<IReadOnlyList<ProductDto>> GetProducts(CancellationToken cancellationToken)
        => masterData.GetProductsAsync(cancellationToken);

    [HttpPost("products")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public Task<ProductDto> CreateProduct(ProductRequest request, CancellationToken cancellationToken)
        => masterData.CreateProductAsync(request, cancellationToken);

    [HttpGet("warehouses")]
    public Task<IReadOnlyList<WarehouseDto>> GetWarehouses(CancellationToken cancellationToken)
        => masterData.GetWarehousesAsync(cancellationToken);

    [HttpPost("warehouses")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public Task<WarehouseDto> CreateWarehouse(WarehouseRequest request, CancellationToken cancellationToken)
        => masterData.CreateWarehouseAsync(request, cancellationToken);
}
