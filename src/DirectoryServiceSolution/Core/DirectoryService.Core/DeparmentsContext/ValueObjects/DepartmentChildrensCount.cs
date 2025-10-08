using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentChildrensCount
{
    public int Value { get; }

    public DepartmentChildrensCount() => Value = 0;

    private DepartmentChildrensCount(int value) => Value = value;

    public static Result<DepartmentChildrensCount> Create(int value) =>
        value < 0
            ? Error.ValidationError(
                "Количество под подразделений подразделения не может быть отрицательным."
            )
            : new DepartmentChildrensCount(value);

    public Result<DepartmentChildrensCount> Add(DepartmentPath parent, Department other) => Add(parent, other.Identifier);

    public Result<DepartmentChildrensCount> Add(
        DepartmentPath parent,
        DepartmentIdentifier otherIdentifier
    )
    {
        if (parent.ContainsIdentifier(otherIdentifier))
            return Error.ConflictError(
                $"Невозможно увеличить количество подразделений у основного подразделения. Дочернее подразделение {otherIdentifier.Value} уже привязано."
            );

        int nextCount = Value + 1;
        return Create(nextCount);
    }
}
