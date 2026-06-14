namespace Eims.Application.Dtos;

public sealed record MovementLineRequest(Guid ProductId, decimal Quantity, decimal? UnitCost);
public sealed record ImportStockRequest(Guid WarehouseId, string ReferenceNo, string? Note, IReadOnlyList<MovementLineRequest> Lines);
public sealed record ExportStockRequest(Guid WarehouseId, string ReferenceNo, string? Note, IReadOnlyList<MovementLineRequest> Lines);
public sealed record TransferStockRequest(Guid FromWarehouseId, Guid ToWarehouseId, string ReferenceNo, string? Note, IReadOnlyList<MovementLineRequest> Lines);
public sealed record ReconcileStockRequest(Guid WarehouseId, Guid ProductId, decimal ActualQuantity, string? Reason);

public sealed record MovementResult(Guid MovementId, string ReferenceNo);
public sealed record StockItemDto(Guid ProductId, string Sku, string ProductName, Guid WarehouseId, string WarehouseCode, decimal Quantity, decimal MinStockLevel, string RowVersion);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total);
