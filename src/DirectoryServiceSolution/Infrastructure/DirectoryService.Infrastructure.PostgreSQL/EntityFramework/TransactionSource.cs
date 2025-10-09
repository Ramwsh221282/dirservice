using DirectoryService.UseCases.Common.Transaction;
using Microsoft.EntityFrameworkCore.Storage;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public sealed class TransactionSource : ITransactionSource
{
    private readonly ServiceDbContext _dbContext;
    private readonly Serilog.ILogger _logger;

    public TransactionSource(ServiceDbContext dbContext, Serilog.ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ITransactionScope> ReceiveTransaction(CancellationToken ct = default)
    {
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        ITransactionScope scope = new TransactionScope(transaction, _logger);
        return scope;
    }
}
