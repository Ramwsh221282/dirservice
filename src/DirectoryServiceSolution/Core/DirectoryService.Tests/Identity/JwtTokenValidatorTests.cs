using System.Text;
using System.Text.Json;
using DirectoryService.Infrastructure.Identity.Jwt;
using Jose;
using ResultLibrary;
using JwtOptions = DirectoryService.Infrastructure.Identity.Jwt.JwtOptions;

namespace DirectoryService.Tests.Identity;

public sealed class JwtTokenValidatorTests
{
    private const string SecretKey = "super-secret-key-for-tests-32-bytes-long!";
    private const string OtherSecretKey = "another-secret-key-32-bytes-long-value!!!";

    private static JwtOptions Options(int accessLifetimeMinutes = 5)
    {
        return new JwtOptions
        {
            SecretKey = SecretKey,
            AccessTokenLifetimeMinutes = accessLifetimeMinutes,
            RefreshTokenLifetimeDays = 7,
        };
    }

    [Fact]
    public void Valid_Token_Is_Accepted_And_Exposes_Payload()
    {
        JwtOptions options = Options();
        JwtProvider provider = new(options);
        JwtTokenValidator validator = new(options);
        Guid userId = Guid.NewGuid();

        GeneratedTokens tokens = provider.GenerateTokens(userId, "ivan");
        Result<ValidatedToken> result = validator.Validate(tokens.AccessToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("ivan", result.Value.Login);
    }

    [Fact]
    public void Expired_Token_Is_Rejected()
    {
        JwtOptions expiredOptions = Options(-1);
        JwtProvider provider = new(expiredOptions);
        JwtTokenValidator validator = new(Options());

        GeneratedTokens tokens = provider.GenerateTokens(Guid.NewGuid(), "ivan");
        Result<ValidatedToken> result = validator.Validate(tokens.AccessToken);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    [Fact]
    public void Token_Signed_With_Foreign_Key_Is_Rejected()
    {
        JwtOptions foreignOptions = new()
        {
            SecretKey = OtherSecretKey,
            AccessTokenLifetimeMinutes = 5,
            RefreshTokenLifetimeDays = 7,
        };

        JwtProvider foreignProvider = new(foreignOptions);
        JwtTokenValidator validator = new(Options());

        GeneratedTokens tokens = foreignProvider.GenerateTokens(Guid.NewGuid(), "ivan");
        Result<ValidatedToken> result = validator.Validate(tokens.AccessToken);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    [Fact]
    public void Token_With_None_Algorithm_Is_Rejected()
    {
        JwtTokenValidator validator = new(Options());

        Dictionary<string, object> payload = new()
        {
            { "sub", Guid.NewGuid().ToString() },
            { "login", "hacker" },
            { "exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
        };

        string forged = JWT.Encode(payload, null, JwsAlgorithm.none);
        Result<ValidatedToken> result = validator.Validate(forged);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    [Fact]
    public void Token_With_Tampered_Payload_Is_Rejected()
    {
        JwtOptions options = Options();
        JwtProvider provider = new(options);
        JwtTokenValidator validator = new(options);

        GeneratedTokens tokens = provider.GenerateTokens(Guid.NewGuid(), "ivan");
        string[] parts = tokens.AccessToken.Split('.');

        Dictionary<string, object> tamperedPayload = new()
        {
            { "sub", Guid.NewGuid().ToString() },
            { "login", "admin" },
            { "exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
        };

        string encoded = Base64Url(JsonSerializer.Serialize(tamperedPayload));
        string tampered = string.Join('.', parts[0], encoded, parts[2]);

        Result<ValidatedToken> result = validator.Validate(tampered);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    [Fact]
    public void Refresh_Token_Is_Not_Accepted_As_Access_Token()
    {
        JwtOptions options = Options();
        JwtProvider provider = new(options);
        JwtTokenValidator validator = new(options);

        GeneratedTokens tokens = provider.GenerateTokens(Guid.NewGuid(), "ivan");
        Result<ValidatedToken> result = validator.Validate(tokens.RefreshToken);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-token")]
    [InlineData("a.b.c")]
    [InlineData("....")]
    public void Malformed_Tokens_Are_Rejected(string token)
    {
        JwtTokenValidator validator = new(Options());

        Result<ValidatedToken> result = validator.Validate(token);

        Assert.True(result.IsFailure);
        Assert.Equal(new UnauthorizedErrorType(), result.Error.Type);
    }

    private static string Base64Url(string value)
    {
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
