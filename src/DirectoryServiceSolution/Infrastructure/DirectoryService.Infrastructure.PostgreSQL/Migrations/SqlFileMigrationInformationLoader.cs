using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Infrastructure;

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations;

public sealed class SqlFileMigrationInformationLoader : IMigrationInformationLoader
{
    private readonly SqlFileMigrationSource _source;

    public SqlFileMigrationInformationLoader(SqlFileMigrationSource source)
    {
        _source = source;
    }

    public SortedList<long, IMigrationInfo> LoadMigrations()
    {
        SortedList<long, IMigrationInfo> migrations = [];

        foreach (SqlMigrationFile file in _source.Discover())
        {
            SqlFileMigration migration = new(file);
            MigrationInfo info = new(
                file.Version,
                file.Name,
                TransactionBehavior.Default,
                false,
                () => migration
            );

            migrations.Add(file.Version, info);
        }

        return migrations;
    }
}
