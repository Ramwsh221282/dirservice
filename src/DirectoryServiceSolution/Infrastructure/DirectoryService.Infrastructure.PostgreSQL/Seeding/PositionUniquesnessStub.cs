using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class PositionUniquesnessStub
{
    private readonly ServiceDbContext _context;

    public PositionUniquesnessStub(ServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PositionNameUniquesness> IsUnique(PositionName name)
    {
        bool hasAny = !(await _context.Positions.AsNoTracking().AnyAsync(p => p.Name == name));
        return new PositionNameUniquesness(hasAny, name.Value);
    }
}
