using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eims.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
        => authService.LoginAsync(request, cancellationToken);

    [HttpPost("users")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public Task<UserDto> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
        => authService.CreateUserAsync(request, cancellationToken);
}
