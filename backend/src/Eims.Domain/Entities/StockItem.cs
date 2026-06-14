using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class StockItem : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal Quantity { get; set; }
}
