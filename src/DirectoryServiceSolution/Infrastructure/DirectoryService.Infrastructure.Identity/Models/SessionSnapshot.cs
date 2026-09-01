namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class SessionSnapshot
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string? RefreshToken { get; }
    public string? AccessToken { get; }
    public DateTime CreatedAt { get; }
    public DateTime AccessTokenExpiresAt { get; }
    public DateTime RefreshTokenExpiresAt { get; }

    private SessionSnapshot(
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

    public static SessionSnapshot FromSession(Session session)
    {
        return new SessionSnapshot(
            session.SessionId,
            session.UserId,
            session.RefreshToken,
            session.AccessToken,
            session.CreatedAt,
            session.AccessTokenExpiresAt,
            session.RefreshTokenExpiresAt
        );
    }
}
