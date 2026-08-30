namespace DirectoryService.Infrastructure.Identity.Jwt;

public sealed record GeneratedTokens(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);

public interface IJwtProvider
{
    GeneratedTokens GenerateTokens(Guid userId, string login);
}
