using System.Data;
using Dapper;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class SqlInterpolationClause : IQueryClause
{
    private readonly DynamicParameters _parameters = new();
    private readonly List<string> _sqlAdditions = [];

    public IQueryClause AddClause<T>(string sql, string parameterName, T value, DbType dbType)
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(parameterName, value, dbType);
        return this;
    }

    public IQueryClause AddClause<T>(string sql, string parameterName, T value)
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(parameterName, value);
        return this;
    }

    public IQueryClause AddClause(string sql)
    {
        _sqlAdditions.Add(sql);
        return this;
    }

    public IQueryClause AddClause(string sql, string parameterName, object value, DbType dbType)
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(parameterName, value, dbType);
        return this;
    }

    public IQueryClause AddClause(string sql, string parameterName, object value)
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(parameterName, value);
        return this;
    }

    public string FormSqlClause(string prefix, string separator)
    {
        return _sqlAdditions.Count == 0
            ? string.Empty
            : prefix + string.Join(separator, _sqlAdditions);
    }

    public string FormRawClause(string sql)
    {
        return _sqlAdditions.Count == 0 ? string.Empty : sql;
    }
}