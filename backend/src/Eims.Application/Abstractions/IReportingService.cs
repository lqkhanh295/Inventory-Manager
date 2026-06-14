using Eims.Application.Dtos;

namespace Eims.Application.Abstractions;

public interface IReportingService
{
    Task<IReadOnlyList<MovementHistoryRow>> GetMovementHistoryAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockValuationRow>> GetStockValuationAsync(CancellationToken cancellationToken);
}
