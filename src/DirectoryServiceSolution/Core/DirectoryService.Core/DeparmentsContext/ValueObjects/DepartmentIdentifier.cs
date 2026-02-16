using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentIdentifier
{
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentIdentifier(string value)
    {
        Value = value;
    }

    public static Result<DepartmentIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string message = "Идентификатор департамента не должен быть пустым";
            return Error.ValidationError(message);
        }

        string formatted = value.Trim();

        if (formatted.GreaterThan(MaxLength))
        {
            string message = $"Идентификатор департамента не должен быть более {MaxLength} символов";
            return Error.ValidationError(message);
        }

        if (!formatted.IsLatinOnly())
        {
            string message = $"Идентификатор департамента должен быть толькой латиницей без пробелов";
            return Error.ValidationError(message);
        }

        return new DepartmentIdentifier(formatted);
    }
}
