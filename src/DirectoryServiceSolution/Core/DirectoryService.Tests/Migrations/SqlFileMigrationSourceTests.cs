using DirectoryService.Infrastructure.PostgreSQL.Migrations;

namespace DirectoryService.Tests.Migrations;

public sealed class SqlFileMigrationSourceTests
{
    private static string CreateDirectory(params string[] fileNames)
    {
        string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directory);

        foreach (string fileName in fileNames)
        {
            File.WriteAllText(Path.Combine(directory, fileName), "-- migrate:up\nSELECT 1;\n");
        }

        return directory;
    }

    [Fact]
    public void Migrations_Are_Ordered_By_Version_Not_By_File_System_Order()
    {
        string directory = CreateDirectory(
            "20260901142426_identity_sessions.sql",
            "20260901124526_positions.sql",
            "20260901125301_departments.sql"
        );

        try
        {
            SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });
            long[] versions = source.Discover().Select(file => file.Version).ToArray();

            Assert.Equal(
                [20260901124526, 20260901125301, 20260901142426],
                versions
            );
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Duplicate_Versions_Are_Rejected()
    {
        string directory = CreateDirectory(
            "20260901124526_first.sql",
            "20260901124526_second.sql"
        );

        try
        {
            SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => source.Discover().ToArray()
            );

            Assert.Contains("одинаковой версией", exception.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Invalid_File_Name_Is_Rejected()
    {
        string directory = CreateDirectory("not-a-migration.sql");

        try
        {
            SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });

            Assert.Throws<InvalidOperationException>(() => source.Discover().ToArray());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Empty_Directory_Is_Rejected()
    {
        string directory = CreateDirectory();

        try
        {
            SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });

            Assert.Throws<InvalidOperationException>(() => source.Discover().ToArray());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Missing_Directory_Is_Rejected()
    {
        string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });

        Assert.Throws<DirectoryNotFoundException>(() => source.Discover().ToArray());
    }

    [Fact]
    public void Shipped_Migrations_Are_Discoverable_And_Ordered()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "migrations");

        Assert.True(
            Directory.Exists(directory),
            $"Каталог migrations не скопирован рядом с артефактом сборки: {directory}"
        );

        SqlFileMigrationSource source = new(new SqlMigrationsOptions { Directory = directory });
        SqlMigrationFile[] files = source.Discover().ToArray();

        Assert.NotEmpty(files);

        long[] versions = files.Select(file => file.Version).ToArray();
        Assert.Equal(versions.OrderBy(version => version).ToArray(), versions);
    }
}
