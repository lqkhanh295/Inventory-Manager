using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class StockMovementLine : AuditableEntity
{
    public Guid StockMovementId { get; set; }
    public StockMovement? StockMovement { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
