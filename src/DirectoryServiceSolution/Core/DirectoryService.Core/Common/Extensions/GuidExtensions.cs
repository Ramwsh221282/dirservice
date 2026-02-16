using ResultLibrary;

namespace DirectoryService.Core.Common.Extensions;

public static class GuidExtensions
{
    public static Result<Guid> ValidGuid(Guid? value)
    {
        if (value == null)
        {
            return Error.ValidationError("Идентификатор не указан.");
        }

        return value.Value.ValidGuid();
    }        

    public static Result<Guid> ValidGuid(this Guid value)
    {
        if (value == Guid.Empty)
        {
            return IncorrectGuidError();
        }

        return value;
    }        

    public static Result<Guid> ValidGuid(this string value)
    {
        if (!Guid.TryParse(value, out Guid guidValue))
        {
            return IncorrectGuidError();
        }

        return ValidGuid(guidValue);
    }        

    private static Error IncorrectGuidError()
    {
        return Error.ValidationError("Идентификатор некорректный");
    }        
}
