using ResultLibrary;

namespace DirectoryService.UseCases.Common.Transaction;

public interface ITransactionScope : IDisposable, IAsyncDisposable
{
    Task<Result> CommitChanges(CancellationToken ct = default, string? methodName = null);
}
