using FluentMigrator;

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations;

public sealed class SqlFileMigration : Migration
{
    private readonly SqlMigrationFile _file;

    public SqlFileMigration(SqlMigrationFile file)
    {
        _file = file;
    }

    public override void Up()
    {
        string script = _file.ReadUpScript();

        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        Execute.Sql(script);
    }

    public override void Down()
    {
        throw new NotSupportedException(
            $"Откат миграции {_file.Version}_{_file.Name} не поддерживается. "
                + "Создайте новую миграцию, отменяющую изменения."
        );
    }
}
