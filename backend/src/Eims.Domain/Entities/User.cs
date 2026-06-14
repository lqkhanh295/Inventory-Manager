using Eims.Domain.Common;
using Eims.Domain.Enums;

namespace Eims.Domain.Entities;

public sealed class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
}
