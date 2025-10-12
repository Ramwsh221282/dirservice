using System.Data;

namespace DirectoryService.UseCases.Common.Database;

public interface IDbConnectionFactory : IDisposable
{
    Task<IDbConnection> Create(CancellationToken ct = default);
    IQueryClause CreateClause();
}
