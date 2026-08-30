using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Locations.GetLocation;

public sealed class GetLocationQuery : IQuery<GetLocationQueryResponse?>
{
    public Guid Id { get; set; }

    public GetLocationQuery(Guid id)
    {
        Id = id;
    }
}
