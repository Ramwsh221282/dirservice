using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public class LocationNameUniquesness
{
    private readonly bool _isUnique;
    private readonly string _nameOfExisting;

    public LocationNameUniquesness(bool isUnique, string nameOfExisting)
    {
        _isUnique = isUnique;
        _nameOfExisting = nameOfExisting;
    }

    public bool IsUnique(LocationName name)
    {
        return !name.Value.Equals(_nameOfExisting) || _isUnique;
    }

    public Error NonUniqueLocationError()
    {
        return Error.ConflictError($"Локация с именем: {_nameOfExisting} уже существует.");
    }
}
