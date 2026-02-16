namespace ResultLibrary;

public sealed record Error(string Message, ErrorType Type)
{
    public static Error ValidationError(string message)
    {
        return new Error(message, new ValidationErrorType());
    }

    public static Error NotFoundError(string message)
    {
        return new Error(message, new ValidationErrorType());
    }        

    public static Error ConflictError(string message)
    {
        return new Error(message, new ConflictErrorType());
    }        

    public static Error ExceptionalError(string message)
    {
        return new Error(message, new ExceptionalErrorType());
    }        

    public static Error EntityDeletedError()
    {
        return new Error("Объект не существует.", new NotFoundErrorType());
    }        

    public static Error NoError()
    {
        return new Error("", new NoErrorType());
    }

    public bool Any()
    {
        return !string.IsNullOrWhiteSpace(Message);
    }
}
