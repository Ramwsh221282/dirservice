namespace DirectoryService.Infrastructure.Identity.Commands.SignIn;

public sealed class SignInUserInfo
{
    public Guid Id { get; }
    public string Login { get; }
    public DateTime CreatedAt { get; }

    public SignInUserInfo(Guid id, string login, DateTime createdAt)
    {
        Id = id;
        Login = login;
        CreatedAt = createdAt;
    }
}

public sealed class SignInTokenInfo
{
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime AccessTokenExpiresAt { get; }
    public DateTime RefreshTokenExpiresAt { get; }

    public SignInTokenInfo(
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

public sealed class SignInResult
{
    public SignInUserInfo User { get; }
    public SignInTokenInfo Token { get; }

    public SignInResult(SignInUserInfo user, SignInTokenInfo token)
    {
        User = user;
        Token = token;
    }
}
