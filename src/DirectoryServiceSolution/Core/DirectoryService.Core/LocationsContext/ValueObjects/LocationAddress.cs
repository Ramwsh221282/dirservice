using System.Text.Json;
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

    public static Result<LocationAddress> Create(IEnumerable<string> parts)
    {
        IEnumerable<Result<LocationAddressPart>> valid = parts.Select(LocationAddressPart.Create);
        if (valid.Any(r => r.IsFailure))
        {
            Result<LocationAddressPart> failed = valid.First();
            return failed.Error;
        }

        return Create(valid.Select(p => p.Value));
    }

    public static LocationAddress FromJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        List<LocationAddressPart> parts = [];
        JsonElement partsElement = document.RootElement.GetProperty(nameof(Parts));
        foreach (JsonElement node in partsElement.EnumerateArray())
        {
            string? addressNodeString = node.GetProperty("Node").GetString();
            if (string.IsNullOrWhiteSpace(addressNodeString))
                throw new Exception("Invalid address part from json.");
            Result<LocationAddressPart> part = LocationAddressPart.Create(addressNodeString);
            if (part.IsFailure)
                throw new Exception("Invalid address part from json.");
            parts.Add(part);
        }

        return new LocationAddress(parts);
    }
}
