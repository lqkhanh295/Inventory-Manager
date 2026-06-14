using Eims.Application.Dtos;

namespace Eims.Application.Abstractions;

public interface IInventoryService
{
    Task<MovementResult> ImportAsync(ImportStockRequest request, CancellationToken cancellationToken);
    Task<MovementResult> ExportAsync(ExportStockRequest request, CancellationToken cancellationToken);
    Task<MovementResult> TransferAsync(TransferStockRequest request, CancellationToken cancellationToken);
    Task<MovementResult> ReconcileAsync(ReconcileStockRequest request, CancellationToken cancellationToken);
    Task<PagedResult<StockItemDto>> GetStockAsync(int page, int pageSize, Guid? productId, Guid? warehouseId, CancellationToken cancellationToken);
}
