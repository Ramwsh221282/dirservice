using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed class LocationsIdSet
{
    private readonly List<LocationId> _ids;

    public IReadOnlyList<LocationId> Ids => _ids;

    private LocationsIdSet(IEnumerable<LocationId> ids)
    {
        _ids = [.. ids];
    }

    public static Result<LocationsIdSet> Create(IEnumerable<Guid> ids)
    {
        List<LocationId> identifiers = [];
        foreach (Guid value in ids)
        {
            LocationId identifier = new(value);
            identifiers.Add(identifier);
        }

        return Create(identifiers);
    }

    public static Result<LocationsIdSet> Create(IEnumerable<LocationId> ids)
    {
        if (!ids.Any())
            return Error.ValidationError("Список идентификаторов локаций был пустым.");

        IEnumerable<LocationId> duplicates = ids.ExtractDuplicates(i => i);
        if (duplicates.Any())
        {
            string[] nonUniqueIdentifiers = [.. duplicates.Select(v => v.Value.ToString())];
            string errorMessage = $"""
                Список идентификаторов локаций должен быть уникален. 
                Повторяющиеся значения: {string.Join(',', nonUniqueIdentifiers)}
                """;
            return Error.ValidationError(errorMessage);
        }

        return new LocationsIdSet(ids);
    }
}
