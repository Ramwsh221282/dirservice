namespace DirectoryService.Infrastructure.Identity.Configuration;

public sealed class IdentityConfig
{
    public const string JwtHashKeyKey = "JWT_HASH_KEY";
    public const string IdentityDbConnectionStringKey = "IDENTITY_DB_CONNECTION_STRING";

    public string JwtHashkey { get; }
    public string IdentityDbConnectionString { get; }

    public IdentityConfig(string jwtHashkey, string identityDbConnectionString)
    {
        JwtHashkey = jwtHashkey;
        IdentityDbConnectionString = identityDbConnectionString;
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

        string? connectionString = Environment.GetEnvironmentVariable(IdentityDbConnectionStringKey);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ApplicationException(
                string.Format("{0} is not specified in environment variables", IdentityDbConnectionStringKey)
            );
        }

        return new IdentityConfig(jwtHashkey, connectionString);
    }
}
