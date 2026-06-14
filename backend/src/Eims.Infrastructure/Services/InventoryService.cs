using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Entities;
using Eims.Domain.Enums;
using Eims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eims.Infrastructure.Services;

public sealed class InventoryService(EimsDbContext db) : IInventoryService
{
    public async Task<MovementResult> ImportAsync(ImportStockRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var movement = new StockMovement
        {
            ReferenceNo = request.ReferenceNo,
            Type = MovementType.Import,
            ToWarehouseId = request.WarehouseId,
            Note = request.Note
        };

        foreach (var line in NormalizeLines(request.Lines))
        {
            var product = await GetProductAsync(line.ProductId, cancellationToken);
            var stock = await GetOrCreateStockAsync(line.ProductId, request.WarehouseId, cancellationToken);
            stock.Quantity += line.Quantity;
            movement.Lines.Add(new StockMovementLine { ProductId = line.ProductId, Quantity = line.Quantity, UnitCost = line.UnitCost ?? product.UnitCost });
        }

        db.StockMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new MovementResult(movement.Id, movement.ReferenceNo);
    }

    public async Task<MovementResult> ExportAsync(ExportStockRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var movement = new StockMovement
        {
            ReferenceNo = request.ReferenceNo,
            Type = MovementType.Export,
            FromWarehouseId = request.WarehouseId,
            Note = request.Note
        };

        foreach (var line in NormalizeLines(request.Lines))
        {
            var product = await GetProductAsync(line.ProductId, cancellationToken);
            var stock = await GetExistingStockAsync(line.ProductId, request.WarehouseId, cancellationToken);
            EnsureSufficient(stock, line.Quantity, product.Sku);
            stock.Quantity -= line.Quantity;
            movement.Lines.Add(new StockMovementLine { ProductId = line.ProductId, Quantity = line.Quantity, UnitCost = line.UnitCost ?? product.UnitCost });
        }

        db.StockMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new MovementResult(movement.Id, movement.ReferenceNo);
    }

    public async Task<MovementResult> TransferAsync(TransferStockRequest request, CancellationToken cancellationToken)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
        {
            throw new InvalidOperationException("Source and destination warehouse must be different.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var movement = new StockMovement
        {
            ReferenceNo = request.ReferenceNo,
            Type = MovementType.Transfer,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Note = request.Note
        };

        foreach (var line in NormalizeLines(request.Lines))
        {
            var product = await GetProductAsync(line.ProductId, cancellationToken);
            var source = await GetExistingStockAsync(line.ProductId, request.FromWarehouseId, cancellationToken);
            EnsureSufficient(source, line.Quantity, product.Sku);
            var destination = await GetOrCreateStockAsync(line.ProductId, request.ToWarehouseId, cancellationToken);
            source.Quantity -= line.Quantity;
            destination.Quantity += line.Quantity;
            movement.Lines.Add(new StockMovementLine { ProductId = line.ProductId, Quantity = line.Quantity, UnitCost = line.UnitCost ?? product.UnitCost });
        }

        db.StockMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new MovementResult(movement.Id, movement.ReferenceNo);
    }

    public async Task<MovementResult> ReconcileAsync(ReconcileStockRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var product = await GetProductAsync(request.ProductId, cancellationToken);
        var stock = await GetOrCreateStockAsync(request.ProductId, request.WarehouseId, cancellationToken);
        var systemQuantity = stock.Quantity;
        var difference = request.ActualQuantity - systemQuantity;
        stock.Quantity = request.ActualQuantity;

        var adjustment = new InventoryAdjustment
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            SystemQuantity = systemQuantity,
            ActualQuantity = request.ActualQuantity,
            Reason = request.Reason
        };

        var movement = new StockMovement
        {
            ReferenceNo = $"ADJ-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Type = MovementType.Adjustment,
            ToWarehouseId = request.WarehouseId,
            Note = request.Reason
        };
        movement.Lines.Add(new StockMovementLine { ProductId = request.ProductId, Quantity = difference, UnitCost = product.UnitCost });
        db.InventoryAdjustments.Add(adjustment);
        db.StockMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new MovementResult(movement.Id, movement.ReferenceNo);
    }

    public async Task<PagedResult<StockItemDto>> GetStockAsync(int page, int pageSize, Guid? productId, Guid? warehouseId, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.StockItems.AsNoTracking().Include(x => x.Product).Include(x => x.Warehouse).AsQueryable();
        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }
        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Product!.Sku)
            .ThenBy(x => x.Warehouse!.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockItemDto(
                x.ProductId,
                x.Product!.Sku,
                x.Product.Name,
                x.WarehouseId,
                x.Warehouse!.Code,
                x.Quantity,
                x.Product.MinStockLevel,
                Convert.ToBase64String(x.RowVersion)))
            .ToListAsync(cancellationToken);

        return new PagedResult<StockItemDto>(items, page, pageSize, total);
    }

    private async Task<Product> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await db.Products.SingleOrDefaultAsync(x => x.Id == productId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException($"Product {productId} was not found or is inactive.");
    }

    private async Task<StockItem> GetExistingStockAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken)
    {
        return await db.StockItems.SingleOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, cancellationToken)
            ?? throw new InvalidOperationException("Stock item does not exist.");
    }

    private async Task<StockItem> GetOrCreateStockAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken)
    {
        var stock = await db.StockItems.SingleOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, cancellationToken);
        if (stock is not null)
        {
            return stock;
        }

        stock = new StockItem { ProductId = productId, WarehouseId = warehouseId, Quantity = 0 };
        db.StockItems.Add(stock);
        return stock;
    }

    private static IReadOnlyList<MovementLineRequest> NormalizeLines(IReadOnlyList<MovementLineRequest> lines)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Movement must contain at least one line.");
        }

        if (lines.Any(x => x.Quantity <= 0))
        {
            throw new InvalidOperationException("Movement quantity must be greater than zero.");
        }

        return lines.GroupBy(x => x.ProductId)
            .Select(x => new MovementLineRequest(x.Key, x.Sum(l => l.Quantity), x.LastOrDefault(l => l.UnitCost.HasValue)?.UnitCost))
            .ToList();
    }

    private static void EnsureSufficient(StockItem stock, decimal requestedQuantity, string sku)
    {
        if (stock.Quantity < requestedQuantity)
        {
            throw new InvalidOperationException($"Insufficient stock for {sku}. Available: {stock.Quantity}, requested: {requestedQuantity}.");
        }
    }
}
