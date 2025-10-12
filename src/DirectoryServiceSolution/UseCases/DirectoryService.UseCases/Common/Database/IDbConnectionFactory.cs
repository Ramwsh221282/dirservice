using System.Data;

namespace DirectoryService.Infrastructure.PostgreSQL.Database;

public interface IDbConnectionFactory : IDisposable
{
    Task<IDbConnection> Create(CancellationToken ct = default);
}
