using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed record CreateLocationCommand(
    string Name,
    IEnumerable<string> AddressParts,
    string TimeZone
)
{
    public IEnumerable<LocationAddressPart> CreateAddressParts()
    {
        return AddressParts.Select(p => LocationAddressPart.Create(p));
    }

    public LocationAddress CreateAddress(IEnumerable<LocationAddressPart> parts)
    {
        return LocationAddress.Create(parts);
    }

    public LocationName CreateLocationName()
    {
        return LocationName.Create(Name);
    }

    public LocationTimeZone CreateTimeZone()
    {
        return LocationTimeZone.Create(TimeZone);
    }
}
