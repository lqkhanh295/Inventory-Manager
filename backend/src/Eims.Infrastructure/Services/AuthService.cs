using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Eims.Domain.Entities;
using Eims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Eims.Infrastructure.Services;

public sealed class AuthService(EimsDbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName && x.IsActive, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var options = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.UserName, user.Role, expiresAt);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(x => x.UserName == request.UserName, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            UserName = request.UserName,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.UserName, user.Role, user.IsActive, Convert.ToBase64String(user.RowVersion));
    }
}
