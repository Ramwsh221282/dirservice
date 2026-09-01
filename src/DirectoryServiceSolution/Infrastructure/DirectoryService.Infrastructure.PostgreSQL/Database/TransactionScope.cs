using System.Data;
using Dapper;
using DirectoryService.UseCases.Common.Transaction;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class TransactionScope : ITransactionScope
{
    private readonly IDbConnection _connection;
    private readonly Serilog.ILogger _logger;
    private bool _completed;
    private bool _disposed;

    private TransactionScope(IDbConnection connection, Serilog.ILogger logger)
    {
        _connection = connection;
        _logger = logger;
        _disposed = false;
    }

    public static async Task<TransactionScope> Begin(
        IDbConnection connection,
        Serilog.ILogger logger,
        CancellationToken ct = default
    )
    {
        CommandDefinition beginCommand = new("BEGIN;", cancellationToken: ct);
        await connection.ExecuteAsync(beginCommand);
        return new TransactionScope(connection, logger);
    }

    public async Task<Result> CommitChanges(
        string? methodName = null,
        CancellationToken ct = default
    )
    {
        if (_completed)
        {
            return Error.ConflictError("Транзакция уже завершена.");
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
            await Rollback();

            if (!string.IsNullOrWhiteSpace(methodName))
            {
                _logger.Fatal("Transaction error at: {Method}", methodName);
                _logger.Fatal("Transaction error info: {Ex}", ex);
            }
            else
            {
                _logger.Fatal("Transaction error info: {Ex}", ex);
            }

            return Error.ConflictError("Ошибка во время транзакции.");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_completed)
        {
            Rollback().GetAwaiter().GetResult();
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
    }

    private async Task Rollback()
    {
        try
        {
            await _connection.ExecuteAsync(new CommandDefinition("ROLLBACK;"));
            _completed = true;
        }
        catch (Exception ex)
        {
            _logger.Error("Transaction rollback failed: {Ex}", ex);
        }
    }
}
