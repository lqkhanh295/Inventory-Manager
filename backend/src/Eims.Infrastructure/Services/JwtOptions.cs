namespace Eims.Infrastructure.Services;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "EIMS";
    public string Audience { get; set; } = "EIMS";
    public string SigningKey { get; set; } = "change-me-to-a-long-secret-key-for-production";
    public int ExpirationMinutes { get; set; } = 120;
}
