using System.Data;
using DirectoryService.UseCases.Common.Transaction;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public sealed class TransactionSource : ITransactionSource
{
    private readonly IDbConnection _connection;
    private readonly Serilog.ILogger _logger;

    public TransactionSource(IDbConnection connection, Serilog.ILogger logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<ITransactionScope> ReceiveTransaction(CancellationToken ct = default)
    {
        return await TransactionScope.Begin(_connection, _logger, ct);
    }
}
