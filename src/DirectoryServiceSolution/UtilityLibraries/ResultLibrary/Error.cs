namespace ResultLibrary;

public sealed record Error(string Message, ErrorType Type)
{
    public static Error ValidationError(string message) =>
        new Error(message, new ValidationErrorType());

    public static Error NotFoundError(string message) =>
        new Error(message, new ValidationErrorType());

    public static Error ConflictError(string message) =>
        new Error(message, new ConflictErrorType());

    public static Error ExceptionalError(string message) =>
        new Error(message, new ExceptionalErrorType());

    public static Error NoError() => new Error("", new NoErrorType());

    public bool Any() => !string.IsNullOrWhiteSpace(Message);
}
