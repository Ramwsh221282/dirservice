namespace DirectoryService.Infrastructure.PostgreSQL.Migrations;

public sealed class SqlMigrationsOptions
{
    public string Directory { get; init; } = "migrations";
}

public sealed class SqlFileMigrationSource
{
    private readonly SqlMigrationsOptions _options;

    public SqlFileMigrationSource(SqlMigrationsOptions options)
    {
        _options = options;
    }

    public string DirectoryPath =>
        Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(AppContext.BaseDirectory, _options.Directory);

    public IEnumerable<SqlMigrationFile> Discover()
    {
        string directory = DirectoryPath;

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Каталог с миграциями не найден: {directory}"
            );
        }

        List<SqlMigrationFile> files = [];
        HashSet<long> versions = [];

        foreach (string path in Directory.EnumerateFiles(directory, "*.sql"))
        {
            if (!SqlMigrationFile.TryCreate(path, out SqlMigrationFile file))
            {
                throw new InvalidOperationException(
                    $"Имя файла миграции должно начинаться с версии: {Path.GetFileName(path)}. "
                        + "Ожидаемый формат: <версия>_<название>.sql"
                );
            }

            if (!versions.Add(file.Version))
            {
                throw new InvalidOperationException(
                    $"Обнаружены две миграции с одинаковой версией {file.Version}."
                );
            }

            files.Add(file);
        }

        if (files.Count == 0)
        {
            throw new InvalidOperationException(
                $"В каталоге {directory} нет файлов миграций."
            );
        }

        files.Sort((left, right) => left.Version.CompareTo(right.Version));
        return files;
    }
}
