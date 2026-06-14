using System.Security.Claims;
using Eims.Application.Abstractions;

namespace Eims.Api;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserName => accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
}
