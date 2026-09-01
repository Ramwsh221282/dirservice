using System.Text.Json;
using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.Infrastructure.PostgreSQL.Snapshots;

public sealed record LocationAddressPartSnapshot(
    string Name,
    string ShortName,
    short AoLevel,
    string Type
)
{
    public static LocationAddressPartSnapshot FromPart(LocationAddressPart part)
    {
        return new LocationAddressPartSnapshot(
            part.Name,
            part.ShortName,
            part.AoLevel,
            part.Type
        );
    }

    public LocationAddressPart ToPart()
    {
        return LocationAddressPart.Create(Name, ShortName, AoLevel, Type);
    }
}

public sealed record LocationAddressSnapshot(
    string FullPath,
    IReadOnlyList<LocationAddressPartSnapshot> Parts
)
{
    public static LocationAddressSnapshot FromAddress(LocationAddress address)
    {
        return new LocationAddressSnapshot(
            address.FullPath,
            [.. address.Parts.Select(LocationAddressPartSnapshot.FromPart)]
        );
    }

    public static LocationAddressSnapshot FromJson(string json)
    {
        LocationAddressSnapshot? snapshot = JsonSerializer.Deserialize<LocationAddressSnapshot>(
            json,
            JsonSerializerOptions.Default
        );

        if (snapshot == null)
        {
            throw new ApplicationException(
                $"Некорректный JSON для {nameof(LocationAddress)}."
            );
        }

        return snapshot;
    }

    public LocationAddress ToAddress()
    {
        return LocationAddress.Create([.. Parts.Select(p => p.ToPart())], FullPath);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
    }

    public bool IsSameAs(LocationAddressSnapshot other)
    {
        if (!string.Equals(FullPath, other.FullPath, StringComparison.Ordinal))
        {
            return false;
        }

        if (Parts.Count != other.Parts.Count)
        {
            return false;
        }

        for (int index = 0; index < Parts.Count; index++)
        {
            if (Parts[index] != other.Parts[index])
            {
                return false;
            }
        }

        return true;
    }
}
