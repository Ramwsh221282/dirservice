namespace DirectoryService.Infrastructure.Identity.Commands.RefreshToken;

public sealed class RefreshTokenResult
{
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime AccessTokenExpiresAt { get; }
    public DateTime RefreshTokenExpiresAt { get; }

    public RefreshTokenResult(
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAt,
        DateTime refreshTokenExpiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
    }
}
