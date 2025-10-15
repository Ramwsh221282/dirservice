using System.Data;
using Dapper;

namespace DirectoryService.UseCases.Common.Database;

public interface IQueryClause
{
    IQueryClause AddClause<T>(string sql, string parameterName, T value, DbType dbType)
        where T : notnull;

    IQueryClause AddClause<T>(string sql, string parameterName, T value)
        where T : notnull;

    IQueryClause AddClause(string sql);
    IQueryClause AddClause(string sql, string parameterName, object value, DbType dbType);
    IQueryClause AddClause(string sql, string parameterName, object value);
    string FormSqlClause(string prefix, string separator);
    string FormRawClause(string sql);
    string FormSeperatedRawClause(string separator);

    public void InjectParameters(DynamicParameters parameters);

    public CommandDefinition FormCommand(
        string sql,
        DynamicParameters parameters,
        CancellationToken ct = default
    );
}
