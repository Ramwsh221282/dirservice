using System.Data;
using Dapper;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class SqlParameter
{
    private readonly object _value;
    private readonly string _name;
    private readonly DbType? _dbType;

    public SqlParameter(string name, object value, DbType? dbType)
    {
        _value = value;
        _name = name;
        _dbType = dbType;
    }

    public void Inject(DynamicParameters parameters)
    {
        if (_dbType != null)
            parameters.Add(_name, _value);
        else
            parameters.Add(_name, _value, _dbType);
    }
}

public sealed class SqlInterpolationClause : IQueryClause
{
    private readonly List<SqlParameter> _parameters = [];
    private readonly List<string> _sqlAdditions = [];

    public IQueryClause AddClause<T>(string sql, string parameterName, T value, DbType dbType)
        where T : notnull
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(new SqlParameter(parameterName, value, dbType));
        return this;
    }

    public IQueryClause AddClause<T>(string sql, string parameterName, T value)
        where T : notnull
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(new SqlParameter(parameterName, value, null));
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
        _parameters.Add(new SqlParameter(parameterName, value, dbType));
        return this;
    }

    public IQueryClause AddClause(string sql, string parameterName, object value)
    {
        _sqlAdditions.Add(sql);
        _parameters.Add(new SqlParameter(parameterName, value, null));
        return this;
    }

    public string FormSqlClause(string prefix, string separator) =>
        _sqlAdditions.Count == 0 ? string.Empty : prefix + string.Join(separator, _sqlAdditions);

    public string FormRawClause(string sql) => _sqlAdditions.Count == 0 ? string.Empty : sql;

    public string FormSeperatedRawClause(string separator) =>
        _sqlAdditions.Count == 0 ? string.Empty : string.Join(separator, _sqlAdditions);

    public void InjectParameters(DynamicParameters parameters)
    {
        foreach (SqlParameter parameter in _parameters)
            parameter.Inject(parameters);
    }

    public CommandDefinition FormCommand(
        string sql,
        DynamicParameters parameters,
        CancellationToken ct = default
    ) => new(sql, parameters, cancellationToken: ct);
}
