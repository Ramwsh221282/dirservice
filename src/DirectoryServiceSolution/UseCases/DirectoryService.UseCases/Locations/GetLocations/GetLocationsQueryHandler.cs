using DirectoryService.Contracts.Locations;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.UseCases.Locations.GetLocations;

public sealed class GetLocationsQueryHandler
    : IQueryHandler<GetLocationsQuery, IEnumerable<LocationsResponse>>
{
    private readonly IReadDbContext _dbContext;

    public GetLocationsQueryHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<LocationsResponse>> Handle(
        GetLocationsQuery query,
        CancellationToken ct = default
    )
    {
        return await _dbContext
            .LocationsRead.Select(l => new LocationsResponse()
            {
                Id = l.Id.Value,
                CreatedAt = l.LifeCycle.CreatedAt,
                Name = l.Name.Value,
                TimeZone = l.TimeZone.Value,
                UpdatedAt = l.LifeCycle.UpdatedAt,
                Address = new LocationAddressParts()
                {
                    Parts = l.Address.Parts.Select(p => new LocationAddressNodeResponse()
                    {
                        Node = p.Node,
                    }),
                },
            })
            .ToListAsync(cancellationToken: ct);
    }
}