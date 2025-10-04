using ResultLibrary;

namespace DirectoryService.Core.Common.Extensions;

public static class GuidExtensions
{
    public static Result<Guid> ValidGuid(Guid? value) =>
        value == null
            ? (Result<Guid>)Error.ValidationError("Идентификатор не указан.")
            : value.Value.ValidGuid();

    public static Result<Guid> ValidGuid(this Guid value) =>
        value == Guid.Empty ? IncorrectGuidError() : value;

    public static Result<Guid> ValidGuid(this string value) =>
        Guid.TryParse(value, out Guid guidValue) ? ValidGuid(guidValue) : IncorrectGuidError();

    private static Error IncorrectGuidError() =>
        Error.ValidationError("Идентификатор некорректный");
}
