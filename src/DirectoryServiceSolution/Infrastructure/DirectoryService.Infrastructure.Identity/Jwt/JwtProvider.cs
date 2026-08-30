using System.Text;
using Jose;

namespace DirectoryService.Infrastructure.Identity.Jwt;

public sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(JwtOptions options)
    {
        _options = options;
    }

    public GeneratedTokens GenerateTokens(Guid userId, string login)
    {
        DateTime now = DateTime.UtcNow;
        DateTime accessTokenExpiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        DateTime refreshTokenExpiresAt = now.AddDays(_options.RefreshTokenLifetimeDays);

        Dictionary<string, object> payload = new()
        {
            { "sub", userId.ToString() },
            { "login", login },
            { "iat", new DateTimeOffset(now).ToUnixTimeSeconds() },
            { "exp", new DateTimeOffset(accessTokenExpiresAt).ToUnixTimeSeconds() },
        };

        byte[] key = Encoding.UTF8.GetBytes(_options.SecretKey);
        string accessToken = JWT.Encode(payload, key, JwsAlgorithm.HS256);
        string refreshToken = GenerateRefreshToken();

        return new GeneratedTokens(accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt);
    }

    private static string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
