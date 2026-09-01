using System.Text;

namespace DirectoryService.Infrastructure.Identity.Configuration;

public sealed class IdentityConfig
{
    public const string JwtHashKeyKey = "JWT_HASH_KEY";
    public const int MinJwtHashKeyBytes = 32;

    public string JwtHashkey { get; }

    public IdentityConfig(string jwtHashkey)
    {
        Validate(jwtHashkey);
        JwtHashkey = jwtHashkey;
    }

    public static IdentityConfig CreateFromEnvironment()
    {
        string? jwtHashkey = Environment.GetEnvironmentVariable(JwtHashKeyKey);
        if (string.IsNullOrWhiteSpace(jwtHashkey))
        {
            throw new ApplicationException(
                string.Format("{0} is not specified in environment variables", JwtHashKeyKey)
            );
        }

        return new IdentityConfig(jwtHashkey);
    }

    private static void Validate(string jwtHashkey)
    {
        if (string.IsNullOrWhiteSpace(jwtHashkey))
        {
            throw new ApplicationException(string.Format("{0} is not specified", JwtHashKeyKey));
        }

        int keyLength = Encoding.UTF8.GetByteCount(jwtHashkey);
        if (keyLength < MinJwtHashKeyBytes)
        {
            throw new ApplicationException(
                string.Format(
                    "{0} is too short for HS256: {1} bytes provided, at least {2} bytes required",
                    JwtHashKeyKey,
                    keyLength,
                    MinJwtHashKeyBytes
                )
            );
        }
    }
}
