using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eims.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ReportsController(IReportingService reporting) : ControllerBase
{
    [HttpGet("movement-history")]
    public Task<IReadOnlyList<MovementHistoryRow>> MovementHistory([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken cancellationToken)
        => reporting.GetMovementHistoryAsync(from, to, cancellationToken);

    [HttpGet("stock-valuation")]
    public Task<IReadOnlyList<StockValuationRow>> StockValuation(CancellationToken cancellationToken)
        => reporting.GetStockValuationAsync(cancellationToken);
}
