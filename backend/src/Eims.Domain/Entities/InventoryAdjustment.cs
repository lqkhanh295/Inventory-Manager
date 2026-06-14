using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class InventoryAdjustment : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Difference => ActualQuantity - SystemQuantity;
    public string? Reason { get; set; }
}
