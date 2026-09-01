using System.Data;
using Npgsql;

namespace DirectoryService.Infrastructure.Identity.Database;

public sealed class NpgSqlIdentityConnectionFactory : IIdentityConnectionFactory, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgSqlIdentityConnectionFactory(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public IDbConnection Create()
    {
        return _dataSource.OpenConnection();
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
