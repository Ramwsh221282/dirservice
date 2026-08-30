namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class Session
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string? RefreshToken { get; }
    public string? AccessToken { get; }
    public DateTime CreatedAt { get; }
    public DateTime AccessTokenExpiresAt { get; }
    public DateTime RefreshTokenExpiresAt { get; }

    public Session(
        Guid sessionId,
        Guid userId,
        string? refreshToken,
        string? accessToken,
        DateTime createdAt,
        DateTime accessTokenExpiresAt,
        DateTime refreshTokenExpiresAt)
    {
        SessionId = sessionId;
        UserId = userId;
        RefreshToken = refreshToken;
        AccessToken = accessToken;
        CreatedAt = createdAt;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
    }

    public static Session CreateNew(
        Guid userId,
        string refreshToken,
        string accessToken,
        DateTime accessTokenExpiresAt,
        DateTime refreshTokenExpiresAt)
    {
        return new Session(
            Guid.NewGuid(),
            userId,
            refreshToken,
            accessToken,
            DateTime.UtcNow,
            accessTokenExpiresAt,
            refreshTokenExpiresAt
        );
    }
}
