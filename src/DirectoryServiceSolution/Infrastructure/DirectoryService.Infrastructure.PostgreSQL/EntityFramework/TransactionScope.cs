using DirectoryService.UseCases.Common.Transaction;
using Microsoft.EntityFrameworkCore.Storage;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public sealed class TransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private readonly Serilog.ILogger _logger;
    private bool _disposed;

    public TransactionScope(IDbContextTransaction transaction, Serilog.ILogger logger)
    {
        _transaction = transaction;
        _logger = logger;
        _disposed = false;
    }

    public async Task<Result> CommitChanges(
        CancellationToken ct = default,
        string? methodName = null
    )
    {
        try
        {
            await _transaction.CommitAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _transaction.RollbackAsync(ct);

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
        if (!_disposed)
        {
            _transaction.Dispose();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
}
