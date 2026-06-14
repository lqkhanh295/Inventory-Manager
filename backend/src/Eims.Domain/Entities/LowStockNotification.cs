using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class LowStockNotification : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinStockLevel { get; set; }
    public bool IsResolved { get; set; }
}
