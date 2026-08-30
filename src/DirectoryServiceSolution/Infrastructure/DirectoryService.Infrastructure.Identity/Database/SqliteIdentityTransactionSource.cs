namespace DirectoryService.Infrastructure.Identity.Database;

public sealed class SqliteIdentityTransactionSource : IIdentityTransactionSource
{
    private readonly IIdentityConnectionFactory _connectionFactory;

    public SqliteIdentityTransactionSource(IIdentityConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IIdentityTransactionScope> ReceiveTransaction(CancellationToken ct = default)
    {
        return await SqliteIdentityTransactionScope.Begin(_connectionFactory, ct);
    }
}
