using Eims.Domain.Enums;

namespace Eims.Application.Dtos;

public sealed record MovementHistoryRow(
    Guid MovementId,
    string ReferenceNo,
    MovementType Type,
    DateTimeOffset PostedAt,
    string? FromWarehouse,
    string? ToWarehouse,
    string Sku,
    string ProductName,
    decimal Quantity,
    decimal UnitCost);

public sealed record StockValuationRow(
    Guid ProductId,
    string Sku,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalValue);
