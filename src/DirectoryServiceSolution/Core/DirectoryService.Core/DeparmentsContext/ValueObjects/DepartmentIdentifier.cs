using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentIdentifier
{
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentIdentifier(string value) => Value = value;

    public static Result<DepartmentIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError("Идентификатор департамента не должен быть пустым");

        string formatted = value.Trim();

        if (formatted.GreaterThan(MaxLength))
            return Error.ValidationError(
                $"Идентификатор департамента не должен быть более {MaxLength} символов"
            );

        if (!formatted.IsLatinOnly())
            return Error.ValidationError(
                $"Идентификатор департамента должен быть толькой латиницей без пробелов"
            );

        return new DepartmentIdentifier(formatted);
    }
}
