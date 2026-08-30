using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Repositories;

public interface IIdentityRepository
{
    Task<Result> Add(User user, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<Result> Delete(User user, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<Result> Update(User user, CancellationToken ct = default, IIdentityTransactionScope? transaction = null);
    Task<User?> Get(
        UserSpecification specification,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null
    );
}
