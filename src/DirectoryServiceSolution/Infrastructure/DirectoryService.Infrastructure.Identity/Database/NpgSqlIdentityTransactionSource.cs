namespace DirectoryService.Infrastructure.Identity.Database;

public sealed class NpgSqlIdentityTransactionSource : IIdentityTransactionSource
{
    private readonly IIdentityConnectionFactory _connectionFactory;

    public NpgSqlIdentityTransactionSource(IIdentityConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IIdentityTransactionScope> ReceiveTransaction(CancellationToken ct = default)
    {
        return await NpgSqlIdentityTransactionScope.Begin(_connectionFactory, ct);
    }
}
