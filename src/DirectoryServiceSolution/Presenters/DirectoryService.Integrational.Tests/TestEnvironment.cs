namespace DirectoryService.Integrational.Tests;

public static class TestEnvironment
{
    private static readonly Lock Sync = new();
    private static bool _configured;

    public static void EnsureConfigured()
    {
        lock (Sync)
        {
            if (_configured)
            {
                return;
            }

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "production");
            Environment.SetEnvironmentVariable("DB_HOST", "localhost");
            Environment.SetEnvironmentVariable("DB_PORT", "5432");
            Environment.SetEnvironmentVariable("DB_USERNAME", "username");
            Environment.SetEnvironmentVariable("DB_PASSWORD", "password");
            Environment.SetEnvironmentVariable("DB_DATABASE", "database");
            Environment.SetEnvironmentVariable("SEQ_HOST", "http://localhost:5341");
            Environment.SetEnvironmentVariable("JWT_HASH_KEY", "integration-tests-jwt-hash-key-0123456789");
            Environment.SetEnvironmentVariable("USE_SEED", "false");

            _configured = true;
        }
    }
}
