using Eims.Domain.Enums;

namespace Eims.Application.Dtos;

public sealed record LoginRequest(string UserName, string Password);
public sealed record LoginResponse(string AccessToken, string UserName, UserRole Role, DateTimeOffset ExpiresAt);
public sealed record CreateUserRequest(string UserName, string Password, UserRole Role);
public sealed record UserDto(Guid Id, string UserName, UserRole Role, bool IsActive, string RowVersion);
