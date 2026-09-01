using System.Globalization;
using System.Text;

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations;

public sealed class SqlMigrationFile
{
    private const string UpMarker = "-- migrate:up";
    private const string DownMarker = "-- migrate:down";

    public long Version { get; }
    public string Name { get; }
    public string Path { get; }

    private SqlMigrationFile(long version, string name, string path)
    {
        Version = version;
        Name = name;
        Path = path;
    }

    public static bool TryCreate(string path, out SqlMigrationFile file)
    {
        file = null!;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        int separatorIndex = fileName.IndexOf('_');

        if (separatorIndex <= 0)
        {
            return false;
        }

        string versionPart = fileName[..separatorIndex];

        if (!long.TryParse(versionPart, NumberStyles.None, CultureInfo.InvariantCulture, out long version))
        {
            return false;
        }

        string name = fileName[(separatorIndex + 1)..];
        file = new SqlMigrationFile(version, name, path);
        return true;
    }

    public string ReadUpScript()
    {
        using FileStream stream = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader reader = new(stream);

        StringBuilder script = new();
        bool insideUpSection = false;
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith(UpMarker, StringComparison.Ordinal))
            {
                insideUpSection = true;
                continue;
            }

            if (trimmed.StartsWith(DownMarker, StringComparison.Ordinal))
            {
                break;
            }

            if (insideUpSection)
            {
                script.AppendLine(line);
            }
        }

        return script.ToString().Trim();
    }
}
