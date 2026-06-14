using Eims.Application.Dtos;

namespace Eims.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
