using System.Data;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public interface IQueryClause
{
    IQueryClause AddClause<T>(string sql, string parameterName, T value, DbType dbType);
    IQueryClause AddClause<T>(string sql, string parameterName, T value);
    IQueryClause AddClause(string sql);
    IQueryClause AddClause(string sql, string parameterName, object value, DbType dbType);
    IQueryClause AddClause(string sql, string parameterName, object value);
}