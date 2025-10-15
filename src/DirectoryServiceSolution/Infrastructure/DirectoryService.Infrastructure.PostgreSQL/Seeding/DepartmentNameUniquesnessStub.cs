using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class DepartmentNameUniquesnessStub
{
    private readonly ServiceDbContext _dbContext;

    public DepartmentNameUniquesnessStub(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasWithName(DepartmentName name)
    {
        bool hasAny = await _dbContext.Departments.AsNoTracking().AnyAsync(d => d.Name == name);
        return hasAny;
    }
}
