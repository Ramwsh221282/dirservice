using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Database;

public interface IIdentityTransactionScope : IAsyncDisposable
{
    Task<Result> Commit(CancellationToken ct = default);
    Task<Result> Rollback(CancellationToken ct = default);
}
