namespace DirectoryService.Infrastructure.Identity.Database;

public interface IIdentityTransactionSource
{
    Task<IIdentityTransactionScope> ReceiveTransaction(CancellationToken ct = default);
}
