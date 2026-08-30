using System.Data;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Common.Database;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class NpgSqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgSqlConnectionFactory(NpgSqlConnectionOptions connectionOptions, ILoggerFactory loggerFactory)
    {
        _dataSource = new NpgsqlDataSourceBuilder(connectionOptions.ConnectionString)
            .UseLoggerFactory(loggerFactory)
            .Build();
    }

    public async Task<IDbConnection> Create(CancellationToken ct = default)
    {
        NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        return connection;
    }

    public IQueryClause CreateClause()
    {
        return new SqlInterpolationClause();
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}
