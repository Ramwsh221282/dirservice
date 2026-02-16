using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentChildrensCount
{
    public int Value { get; }

    public DepartmentChildrensCount()
    {
        Value = 0;
    }

    private DepartmentChildrensCount(int value)
    {
        Value = value;
    }

    public static Result<DepartmentChildrensCount> Create(int value)
    {
        if (value < 0)
        {
            string message = "Количество под подразделений подразделения не может быть отрицательным.";
            return Error.ValidationError(message);
        }

        return new DepartmentChildrensCount(value);
    }

    public Result<DepartmentChildrensCount> Reduce()
    {
        int nextValue = Value - 1;
        return Create(nextValue);
    }

    public Result<DepartmentChildrensCount> Add(DepartmentPath parent, Department other)
    {
        return Add(parent, other.Identifier);
    }        

    public Result<DepartmentChildrensCount> Add(DepartmentPath parent,DepartmentIdentifier otherIdentifier)
    {
        if (parent.ContainsIdentifier(otherIdentifier))
        {
            string message = $"Невозможно увеличить количество подразделений у основного подразделения. Дочернее подразделение {otherIdentifier.Value} уже привязано.";
            return Error.ConflictError(message);
        }

        int nextCount = Value + 1;
        return Create(nextCount);
    }
}
