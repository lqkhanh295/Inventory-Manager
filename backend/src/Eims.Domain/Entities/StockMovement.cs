using Eims.Domain.Common;
using Eims.Domain.Enums;

namespace Eims.Domain.Entities;

public sealed class StockMovement : AuditableEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public MovementStatus Status { get; set; } = MovementStatus.Posted;
    public Guid? FromWarehouseId { get; set; }
    public Warehouse? FromWarehouse { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public Warehouse? ToWarehouse { get; set; }
    public string? Note { get; set; }
    public ICollection<StockMovementLine> Lines { get; set; } = new List<StockMovementLine>();
}
