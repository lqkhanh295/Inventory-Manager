using Eims.Domain.Common;

namespace Eims.Domain.Entities;

public sealed class Warehouse : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Warehouse? Parent { get; set; }
    public ICollection<Warehouse> Children { get; set; } = new List<Warehouse>();
}
