using ResultLibrary;

namespace DirectoryService.Core.PositionsContext.ValueObjects;

public sealed class PositionNameUniquesness
{
    private readonly bool _isUnique;
    private readonly string _existingName;

    public PositionNameUniquesness(bool isUnique, string existingName)
    {
        _isUnique = isUnique;
        _existingName = existingName;
    }

    public bool IsUnique(PositionName name) => 
        _isUnique && !_existingName.Equals(name.Value);

    public Error NotUniqueNameError() => 
        Error.ConflictError($"Позиция с наименованием: {_existingName} уже существует в системе.");
}