namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationAddress
{
    private readonly List<LocationAddressPart> _parts = [];
    public IReadOnlyList<LocationAddressPart> Parts => _parts;

    private LocationAddress()
    {
        // ef core
    }

    private LocationAddress(List<LocationAddressPart> parts)
    {
        _parts = parts;
    }

    public static LocationAddress Create(IEnumerable<LocationAddressPart> parts)
    {
        List<LocationAddressPart> asList = parts.ToList();
        if (asList.Count == 0)
            throw new ArgumentException("Адрес не может состоять без частей адреса.");
        return new LocationAddress(asList);
    }
}
