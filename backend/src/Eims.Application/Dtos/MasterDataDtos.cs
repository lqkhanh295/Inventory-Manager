namespace Eims.Application.Dtos;

public sealed record CategoryRequest(string Name, Guid? ParentId);
public sealed record CategoryDto(Guid Id, string Name, Guid? ParentId, string RowVersion);

public sealed record ProductRequest(string Sku, string Name, Guid CategoryId, decimal UnitCost, decimal MinStockLevel, bool IsActive = true);
public sealed record ProductDto(Guid Id, string Sku, string Name, Guid CategoryId, decimal UnitCost, decimal MinStockLevel, bool IsActive, string RowVersion);

public sealed record WarehouseRequest(string Code, string Name, Guid? ParentId);
public sealed record WarehouseDto(Guid Id, string Code, string Name, Guid? ParentId, string RowVersion);
