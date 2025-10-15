using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class LocationNameUniquesnessStub
{
    private readonly ServiceDbContext _context;

    public LocationNameUniquesnessStub(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<LocationNameUniquesness> IsUnique(LocationName name)
    {
        bool isUnique = !(await _context.Locations.AsNoTracking().AnyAsync(l => l.Name == name));
        return new LocationNameUniquesness(isUnique, name.Value);
    }
}