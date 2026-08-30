namespace DirectoryService.Infrastructure.Identity.Jwt;

public sealed class JwtOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; init; } = 5;
    public int RefreshTokenLifetimeDays { get; init; } = 7;
}
