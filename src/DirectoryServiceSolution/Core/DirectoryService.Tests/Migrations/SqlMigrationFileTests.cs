using DirectoryService.Infrastructure.PostgreSQL.Migrations;

namespace DirectoryService.Tests.Migrations;

public sealed class SqlMigrationFileTests
{
    private static string WriteTempFile(string name, string content)
    {
        string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Version_And_Name_Are_Parsed_From_File_Name()
    {
        string path = WriteTempFile("20260901124526_positions_initial.sql", "-- migrate:up\nSELECT 1;\n");

        try
        {
            Assert.True(SqlMigrationFile.TryCreate(path, out SqlMigrationFile file));
            Assert.Equal(20260901124526, file.Version);
            Assert.Equal("positions_initial", file.Name);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Theory]
    [InlineData("no-version-prefix.sql")]
    [InlineData("_leading_underscore.sql")]
    [InlineData("abc_not_a_number.sql")]
    [InlineData("20260901124526.sql")]
    public void Invalid_File_Names_Are_Rejected(string fileName)
    {
        string path = WriteTempFile(fileName, "-- migrate:up\nSELECT 1;\n");

        try
        {
            Assert.False(SqlMigrationFile.TryCreate(path, out _));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Fact]
    public void Only_Up_Section_Is_Read()
    {
        const string content = """
            -- migrate:up
            CREATE TABLE example (id INT);

            -- migrate:down
            DROP TABLE example;
            """;

        string path = WriteTempFile("20260101000000_example.sql", content);

        try
        {
            Assert.True(SqlMigrationFile.TryCreate(path, out SqlMigrationFile file));
            string script = file.ReadUpScript();

            Assert.Contains("CREATE TABLE example", script);
            Assert.DoesNotContain("DROP TABLE", script);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Fact]
    public void Multi_Statement_Up_Section_Is_Read_Fully()
    {
        const string content = """
            -- migrate:up
            CREATE TABLE a (id INT);

            CREATE INDEX idx_a ON a (id);

            -- migrate:down
            DROP TABLE a;
            """;

        string path = WriteTempFile("20260101000000_multi.sql", content);

        try
        {
            Assert.True(SqlMigrationFile.TryCreate(path, out SqlMigrationFile file));
            string script = file.ReadUpScript();

            Assert.Contains("CREATE TABLE a", script);
            Assert.Contains("CREATE INDEX idx_a", script);
            Assert.DoesNotContain("DROP TABLE", script);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Fact]
    public void File_Without_Up_Marker_Yields_Empty_Script()
    {
        string path = WriteTempFile("20260101000000_empty.sql", "SELECT 1;\n");

        try
        {
            Assert.True(SqlMigrationFile.TryCreate(path, out SqlMigrationFile file));
            Assert.Empty(file.ReadUpScript());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [Fact]
    public void Empty_Up_Section_Yields_Empty_Script()
    {
        const string content = """
            -- migrate:up

            -- migrate:down
            DROP TABLE example;
            """;

        string path = WriteTempFile("20260101000000_empty_up.sql", content);

        try
        {
            Assert.True(SqlMigrationFile.TryCreate(path, out SqlMigrationFile file));
            Assert.Empty(file.ReadUpScript());
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }
}
