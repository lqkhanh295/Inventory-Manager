using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Entities;
using Eims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Eims.Infrastructure.Services;

public sealed class MasterDataService(EimsDbContext db, IMemoryCache cache) : IMasterDataService
{
    private const string ProductCacheKey = "mdm-products";

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await db.Categories.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.Id, x.Name, x.ParentId, Convert.ToBase64String(x.RowVersion)))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category { Name = request.Name, ParentId = request.ParentId };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.ParentId, Convert.ToBase64String(category.RowVersion));
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(ProductCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await db.Products.AsNoTracking()
                .OrderBy(x => x.Sku)
                .Select(x => new ProductDto(x.Id, x.Sku, x.Name, x.CategoryId, x.UnitCost, x.MinStockLevel, x.IsActive, Convert.ToBase64String(x.RowVersion)))
                .ToListAsync(cancellationToken);
        }) ?? [];
    }

    public async Task<ProductDto> CreateProductAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            CategoryId = request.CategoryId,
            UnitCost = request.UnitCost,
            MinStockLevel = request.MinStockLevel,
            IsActive = request.IsActive
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        cache.Remove(ProductCacheKey);
        return new ProductDto(product.Id, product.Sku, product.Name, product.CategoryId, product.UnitCost, product.MinStockLevel, product.IsActive, Convert.ToBase64String(product.RowVersion));
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(CancellationToken cancellationToken)
    {
        return await db.Warehouses.AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new WarehouseDto(x.Id, x.Code, x.Name, x.ParentId, Convert.ToBase64String(x.RowVersion)))
            .ToListAsync(cancellationToken);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(WarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse { Code = request.Code, Name = request.Name, ParentId = request.ParentId };
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.ParentId, Convert.ToBase64String(warehouse.RowVersion));
    }
}
