using System.Text.Json;
using DirectoryService.Core.Common.Extensions;
using DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationAddress
{
    public string FullPath { get; } = null!;

    private readonly List<LocationAddressPart> _parts = [];
    public IReadOnlyList<LocationAddressPart> Parts => _parts;

    private LocationAddress(IEnumerable<LocationAddressPart> parts, string fullPath)
    {
        FullPath = fullPath;
        _parts = [.. parts];
    }

    private LocationAddress()
    {
        // ef
    }

    public static Result<LocationAddress> Create(IEnumerable<LocationAddressPart> parts)
    {
        if (!ContainsAoLevel(parts, SubjectLocationElement.AoLevel))
            return Error.ValidationError("Адрес не содержит субъект.");

        if (AoLevelRepeated(parts, SubjectLocationElement.AoLevel))
            return Error.ValidationError("Адрес не может содержать более 1 субъекта");

        if (!ContainsAoLevel(parts, MunicipalLocationElement.AoLevel))
            return Error.ValidationError("Адрес не содержит населенный пункт.");

        if (AoLevelRepeated(parts, MunicipalLocationElement.AoLevel))
            return Error.ValidationError("Адрес не может содержать более 1 населенного пункта");

        if (!ContainsAoLevel(parts, StreetLocationElement.AoLevel))
            return Error.ValidationError("Адрес не содержит улицу");

        if (AoLevelRepeated(parts, StreetLocationElement.AoLevel))
            return Error.ValidationError("Адрес не может содержать более 1 улицы");

        if (!ContainsAoLevel(parts, BuildingLocationElement.AoLevel))
            return Error.ValidationError("Адрес не содержит строение");

        if (AoLevelRepeated(parts, BuildingLocationElement.AoLevel))
            return Error.ValidationError("Адрес не может содержать более 1 строения.");

        LocationAddressPart[] sorted = [.. parts.OrderBy(p => p.AoLevel)];
        string fullPath = string.Join(", ", sorted.Select(i => i.Name));
        return new LocationAddress(sorted, fullPath);
    }

    public static Result<LocationAddress> Create(IEnumerable<string> parts)
    {
        IEnumerable<string> duplicates = parts.ExtractDuplicates(p => p);
        if (duplicates.Any())
        {
            string errorMessage = $"""
                У адреса локации найдены дублирующиеся узлы: 
                {string.Join(", ", duplicates)}
                """;
            return Error.ValidationError(errorMessage);
        }

        Result<LocationAddressPart>[] results = parts.Select(LocationAddressPart.Create).ToArray();
        Result<LocationAddressPart>? failed = results.FirstOrDefault(r => r.IsFailure);
        if (failed != null)
            return failed.Error;

        LocationAddressPart[] valid = results.Select(r => r.Value).ToArray();
        return Create(valid);
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

        return new LocationAddress([], string.Empty);
    }

    private static bool AoLevelRepeated(IEnumerable<LocationAddressPart> parts, short aoLevel)
    {
        bool repeated = parts.Count(p => p.AoLevel == aoLevel && p.AoLevel > 1) > 1;
        return repeated;
    }

    private static bool ContainsAoLevel(IEnumerable<LocationAddressPart> parts, short aoLevel)
    {
        return parts.Any(p => p.AoLevel == aoLevel);
    }
}
