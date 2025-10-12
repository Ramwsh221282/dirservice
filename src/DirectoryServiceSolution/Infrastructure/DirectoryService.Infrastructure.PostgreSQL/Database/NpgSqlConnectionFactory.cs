using System.Data;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Common.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class NpgSqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgSqlConnectionFactory(
        IOptions<NpgSqlConnectionOptions> connectionOptions,
        ILoggerFactory loggerFactory
    )
    {
        NpgSqlConnectionOptions options = connectionOptions.Value;
        _dataSource = new NpgsqlDataSourceBuilder(options.ConnectionString)
            .UseLoggerFactory(loggerFactory)
            .Build();
    }

    public async Task<IDbConnection> Create(CancellationToken ct = default)
    {
        NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        return connection;
    }

    public IQueryClause CreateClause() => new SqlInterpolationClause();

    public void Dispose() => _dataSource.Dispose();

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}
