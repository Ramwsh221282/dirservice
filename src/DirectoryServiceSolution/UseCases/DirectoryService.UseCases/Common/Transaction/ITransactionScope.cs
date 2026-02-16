using ResultLibrary;

namespace DirectoryService.UseCases.Common.Transaction;

public interface ITransactionScope : IDisposable, IAsyncDisposable
{
    Task<Result> CommitChanges(string? methodName = null, CancellationToken ct = default);
}
