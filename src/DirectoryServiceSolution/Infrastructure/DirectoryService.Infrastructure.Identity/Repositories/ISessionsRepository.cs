using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Repositories;

public interface ISessionsRepository
{
    Task<Result> Add(Session session, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<Result> Delete(Session session, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<Result> Update(Session session, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<Session?> Get(
        SessionSpecification specification,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null
    );

    Task<Result> ClearExpiredAccessTokens(CancellationToken ct = default);
}
