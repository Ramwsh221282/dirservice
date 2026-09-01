using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentName
{
    public const short MinLength = 3;
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string message = "Название подразделения не может быть пустым";
            return Error.ValidationError(message);
        }


        string formatted = value.Trim();

        if (formatted.LessThan(MinLength))
        {
            string message = $"Название подразделения не может быть менее {MinLength} символов";
            return Error.ValidationError(message);
        }

        if (formatted.GreaterThan(MaxLength))
        {
            string message = $"Название подразделения не может быть более {MaxLength} символов";
            return Error.ValidationError(message);
        }

        return new DepartmentName(formatted);
    }
}
