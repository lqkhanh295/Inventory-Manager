using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal UnitCost { get; set; }
    public decimal MinStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
