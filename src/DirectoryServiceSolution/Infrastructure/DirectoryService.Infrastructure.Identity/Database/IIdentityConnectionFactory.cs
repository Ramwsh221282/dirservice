using System.Data;

namespace DirectoryService.Infrastructure.Identity.Database;

public interface IIdentityConnectionFactory
{
    IDbConnection Create();
}
