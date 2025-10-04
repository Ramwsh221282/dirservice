using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationAddress
{
    private readonly List<LocationAddressPart> _parts = [];
    public IReadOnlyList<LocationAddressPart> Parts => _parts;

    private LocationAddress()
    {
        // ef core
    }

    private LocationAddress(List<LocationAddressPart> parts) => _parts = parts;

    public static Result<LocationAddress> Create(IEnumerable<LocationAddressPart> parts)
    {
        List<LocationAddressPart> asList = [.. parts];
        return asList.IsEmpty()
            ? Error.ValidationError("Адрес не может состоять без частей адреса.")
            : new LocationAddress(asList);
    }
}
