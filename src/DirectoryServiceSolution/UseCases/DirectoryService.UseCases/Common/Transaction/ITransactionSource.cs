namespace DirectoryService.UseCases.Common.Transaction;

public interface ITransactionSource
{
    Task<ITransactionScope> ReceiveTransaction(CancellationToken ct = default);
}
