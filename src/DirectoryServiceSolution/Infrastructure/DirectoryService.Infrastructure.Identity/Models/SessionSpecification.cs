namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class SessionSpecification
{
    public Guid? SessionId { get; }
    public Guid? UserId { get; }
    public string? RefreshToken { get; }
    public string? AccessToken { get; }
    public bool IsEmpty =>
        SessionId == null && UserId == null && RefreshToken == null && AccessToken == null;

    private SessionSpecification(Guid? sessionId, Guid? userId, string? refreshToken, string? accessToken)
    {
        SessionId = sessionId;
        UserId = userId;
        RefreshToken = refreshToken;
        AccessToken = accessToken;
    }

    public static SessionSpecification Empty()
    {
        return new SessionSpecification(null, null, null, null);
    }

    public SessionSpecification WithSessionId(Guid sessionId)
    {
        return new SessionSpecification(sessionId, UserId, RefreshToken, AccessToken);
    }

    public SessionSpecification WithUserId(Guid userId)
    {
        return new SessionSpecification(SessionId, userId, RefreshToken, AccessToken);
    }

    public SessionSpecification WithRefreshToken(string refreshToken)
    {
        return new SessionSpecification(SessionId, UserId, refreshToken, AccessToken);
    }

    public SessionSpecification WithAccessToken(string accessToken)
    {
        return new SessionSpecification(SessionId, UserId, RefreshToken, accessToken);
    }
}
