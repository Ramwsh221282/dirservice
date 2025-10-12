using DirectoryService.Core.LocationsContext;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }
}