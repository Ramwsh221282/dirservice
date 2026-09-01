using System.Data;
using Dapper;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Database;

internal sealed class NpgSqlIdentityTransactionScope : IIdentityTransactionScope, IInternalConnectionAccessor
{
    private readonly IDbConnection _connection;
    private bool _completed;
    private bool _disposed;

    private NpgSqlIdentityTransactionScope(IDbConnection connection)
    {
        _connection = connection;
    }

    public IDbConnection Connection => _connection;

    public static async Task<NpgSqlIdentityTransactionScope> Begin(
        IIdentityConnectionFactory connectionFactory,
        CancellationToken ct = default)
    {
        IDbConnection connection = connectionFactory.Create();
        CommandDefinition beginCommand = new("BEGIN;", cancellationToken: ct);
        await connection.ExecuteAsync(beginCommand);

        return new NpgSqlIdentityTransactionScope(connection);
    }

    public async Task<Result> Commit(CancellationToken ct = default)
    {
        if (_completed)
        {
            return Error.ConflictError("Транзакция уже завершена (commit/rollback).");
        }

        try
        {
            CommandDefinition commitCommand = new("COMMIT;", cancellationToken: ct);
            await _connection.ExecuteAsync(commitCommand);
            _completed = true;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Error.ExceptionalError($"Не удалось подтвердить транзакцию: {ex.Message}");
        }
    }

    public async Task<Result> Rollback(CancellationToken ct = default)
    {
        if (_completed)
        {
            return Error.ConflictError("Транзакция уже завершена (commit/rollback).");
        }

        try
        {
            CommandDefinition rollbackCommand = new("ROLLBACK;", cancellationToken: ct);
            await _connection.ExecuteAsync(rollbackCommand);
            _completed = true;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Error.ExceptionalError($"Не удалось откатить транзакцию: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_completed)
        {
            await Rollback();
        }

        _connection.Dispose();
    }
}
