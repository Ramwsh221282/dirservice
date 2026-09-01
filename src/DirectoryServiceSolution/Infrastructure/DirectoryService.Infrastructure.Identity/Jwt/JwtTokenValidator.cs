using System.Text;
using System.Text.Json;
using Jose;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Jwt;

public sealed class JwtTokenValidator : ITokenValidator
{
    private readonly JwtOptions _options;

    public JwtTokenValidator(JwtOptions options)
    {
        _options = options;
    }

    public Result<ValidatedToken> Validate(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return InvalidTokenError();
        }

        string payload;
        try
        {
            byte[] key = Encoding.UTF8.GetBytes(_options.SecretKey);
            payload = JWT.Decode(accessToken.Trim(), key, JwsAlgorithm.HS256);
        }
        catch (JoseException)
        {
            return InvalidTokenError();
        }
        catch (ArgumentException)
        {
            return InvalidTokenError();
        }
        catch (FormatException)
        {
            return InvalidTokenError();
        }

        using JsonDocument document = Parse(payload);
        if (document == null)
        {
            return InvalidTokenError();
        }

        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return InvalidTokenError();
        }

        if (!root.TryGetProperty("sub", out JsonElement subElement))
        {
            return InvalidTokenError();
        }

        if (!Guid.TryParse(subElement.GetString(), out Guid userId))
        {
            return InvalidTokenError();
        }

        if (!root.TryGetProperty("login", out JsonElement loginElement))
        {
            return InvalidTokenError();
        }

        string? login = loginElement.GetString();
        if (string.IsNullOrWhiteSpace(login))
        {
            return InvalidTokenError();
        }

        if (!root.TryGetProperty("exp", out JsonElement expElement))
        {
            return InvalidTokenError();
        }

        if (!expElement.TryGetInt64(out long exp))
        {
            return InvalidTokenError();
        }

        DateTime expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
        if (expiresAt <= DateTime.UtcNow)
        {
            return new Error("Срок действия access-токена истёк.", new UnauthorizedErrorType());
        }

        return new ValidatedToken(userId, login, expiresAt);
    }

    private static JsonDocument Parse(string payload)
    {
        try
        {
            return JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            return null!;
        }
    }

    private static Error InvalidTokenError()
    {
        return new Error("Недействительный access-токен.", new UnauthorizedErrorType());
    }
}
