using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eims.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    [HttpGet("stock")]
    public Task<PagedResult<StockItemDto>> GetStock([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? productId = null, [FromQuery] Guid? warehouseId = null, CancellationToken cancellationToken = default)
        => inventory.GetStockAsync(page, pageSize, productId, warehouseId, cancellationToken);

    [HttpPost("import")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)},{nameof(UserRole.Staff)}")]
    public Task<MovementResult> Import(ImportStockRequest request, CancellationToken cancellationToken)
        => inventory.ImportAsync(request, cancellationToken);

    [HttpPost("export")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)},{nameof(UserRole.Staff)}")]
    public Task<MovementResult> Export(ExportStockRequest request, CancellationToken cancellationToken)
        => inventory.ExportAsync(request, cancellationToken);

    [HttpPost("transfer")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public Task<MovementResult> Transfer(TransferStockRequest request, CancellationToken cancellationToken)
        => inventory.TransferAsync(request, cancellationToken);

    [HttpPost("reconcile")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public Task<MovementResult> Reconcile(ReconcileStockRequest request, CancellationToken cancellationToken)
        => inventory.ReconcileAsync(request, cancellationToken);
}
