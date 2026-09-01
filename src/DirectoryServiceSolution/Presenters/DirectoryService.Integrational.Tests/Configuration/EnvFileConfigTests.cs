using DirectoryService.Infrastructure.Identity.Configuration;

namespace DirectoryService.Integrational.Tests.Configuration;

public sealed class EnvFileConfigTests
{
    private static string WriteEnvFile(params string[] lines)
    {
        string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(path, string.Join('\n', lines));
        return path;
    }

    private static string[] ValidLines()
    {
        return
        [
            "DB_HOST=localhost",
            "DB_PORT=5432",
            "DB_USERNAME=user",
            "DB_PASSWORD=password",
            "DB_DATABASE=database",
            "SEQ_HOST=http://localhost:5341",
            "USE_SEED=false",
            "JWT_HASH_KEY=dev-only-jwt-hash-key-change-me-0123456789",
        ];
    }

    [Fact]
    public void Shipped_Env_File_Is_Parsed_Successfully()
    {
        string path = Path.Combine(AppContext.BaseDirectory, ".env");

        Assert.True(File.Exists(path), $"Файл .env не найден рядом с артефактом сборки: {path}");

        ApplicationConfig config = ApplicationConfig.CreateFromEnvFile(path);

        Assert.False(string.IsNullOrWhiteSpace(config.Database.HostName));
        Assert.True(config.Database.Port > 0);
        Assert.False(string.IsNullOrWhiteSpace(config.Identity.JwtHashkey));
    }

    [Fact]
    public void Valid_Env_File_Produces_Expected_Values()
    {
        string path = WriteEnvFile(ValidLines());

        try
        {
            ApplicationConfig config = ApplicationConfig.CreateFromEnvFile(path);

            Assert.Equal("localhost", config.Database.HostName);
            Assert.Equal(5432, config.Database.Port);
            Assert.Equal("user", config.Database.UserName);
            Assert.Equal("database", config.Database.DatabaseName);
            Assert.Equal("http://localhost:5341", config.Seq.Host);
            Assert.False(config.Seed.UseSeed);
            Assert.Equal("dev-only-jwt-hash-key-change-me-0123456789", config.Identity.JwtHashkey);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Env_File_With_Short_Jwt_Key_Is_Rejected()
    {
        string[] lines =
        [
            "DB_HOST=localhost",
            "DB_PORT=5432",
            "DB_USERNAME=user",
            "DB_PASSWORD=password",
            "DB_DATABASE=database",
            "SEQ_HOST=http://localhost:5341",
            "USE_SEED=false",
            "JWT_HASH_KEY=too-short",
        ];

        string path = WriteEnvFile(lines);

        try
        {
            ApplicationException exception = Assert.Throws<ApplicationException>(
                () => ApplicationConfig.CreateFromEnvFile(path)
            );

            Assert.Contains(IdentityConfig.JwtHashKeyKey, exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Env_File_Without_Jwt_Key_Is_Rejected()
    {
        string[] lines =
        [
            "DB_HOST=localhost",
            "DB_PORT=5432",
            "DB_USERNAME=user",
            "DB_PASSWORD=password",
            "DB_DATABASE=database",
            "SEQ_HOST=http://localhost:5341",
            "USE_SEED=false",
        ];

        string path = WriteEnvFile(lines);

        try
        {
            Assert.Throws<ApplicationException>(() => ApplicationConfig.CreateFromEnvFile(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Env_File_With_Blank_Line_Is_Rejected()
    {
        string[] lines =
        [
            "DB_HOST=localhost",
            string.Empty,
            "DB_PORT=5432",
        ];

        string path = WriteEnvFile(lines);

        try
        {
            Assert.Throws<ApplicationException>(() => ApplicationConfig.CreateFromEnvFile(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Env_File_With_Comment_Line_Is_Rejected()
    {
        string[] lines =
        [
            "# комментарий",
            "DB_HOST=localhost",
        ];

        string path = WriteEnvFile(lines);

        try
        {
            Assert.Throws<ApplicationException>(() => ApplicationConfig.CreateFromEnvFile(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
