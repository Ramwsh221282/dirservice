using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.UseCases.Positions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Positions;

public sealed class PositionsRepository : IPositionsRepository
{
    private readonly ServiceDbContext _dbContext;

    public PositionsRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Add(Position position, CancellationToken ct = default)
    {
        await _dbContext.Positions.AddAsync(position, ct);
    }

    public async Task<PositionNameUniquesness> IsUnique(PositionName name, CancellationToken ct = default)
    {
        bool hasAny = await _dbContext.Positions.AsNoTracking().AnyAsync(p => p.Name == name, cancellationToken: ct);
        return hasAny ? 
            new PositionNameUniquesness(false, name.Value) 
            : new PositionNameUniquesness(true, "");
    }
}