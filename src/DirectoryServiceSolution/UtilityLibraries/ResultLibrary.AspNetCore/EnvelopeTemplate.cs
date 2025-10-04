using System.Net;

namespace ResultLibrary.AspNetCore;

public record EnvelopeTemplate
{
    public string MethodName { get; }
    public IEnumerable<string> Errors { get; }
    public DateTime TimeGenerated { get; }
    public int OperationStatus { get; }

    protected EnvelopeTemplate(
        string methodName,
        IEnumerable<string> errors,
        DateTime timeGenerated,
        int operationStatus
    )
    {
        MethodName = methodName;
        Errors = errors;
        TimeGenerated = timeGenerated;
        OperationStatus = operationStatus;
    }

    public static EnvelopeTemplate FromResult(Result result, string methodName) =>
        result switch
        {
            ErrorsCollection col => FromErrorsCollection(col, methodName),
            _ => FromSingleResult(result, methodName),
        };

    private static EnvelopeTemplate FromSingleResult(Result result, string methodName)
    {
        DateTime timeGenerated = DateTime.UtcNow;
        IEnumerable<string> errors = [result.Error.Message];
        int code = StatusCodeAsInteger(DispatchOperationStatus(result.Error.Type));
        return new EnvelopeTemplate(methodName, errors, timeGenerated, code);
    }

    private static EnvelopeTemplate FromSingleResult<T>(Result<T> result, string methodName) =>
        FromResult(result, methodName);

    private static EnvelopeTemplate FromErrorsCollection(ErrorsCollection errors, string methodName)
    {
        DateTime timeGenerated = DateTime.UtcNow;
        IEnumerable<string> errorStrings = errors.ErrorStrings();
        int code = StatusCodeAsInteger(DispatchOperationStatus(errors.GeneralErrorType()));
        return new EnvelopeTemplate(methodName, errorStrings, timeGenerated, code);
    }

    private static int StatusCodeAsInteger(HttpStatusCode code) => (int)code;

    private static HttpStatusCode DispatchOperationStatus(ErrorType errorType) =>
        errorType switch
        {
            ConflictErrorType => HttpStatusCode.Conflict,
            ExceptionalErrorType => HttpStatusCode.InternalServerError,
            NotFoundErrorType => HttpStatusCode.NotFound,
            ValidationErrorType => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.OK,
        };
}

public sealed record EnvelopeTemplate<T> : EnvelopeTemplate
{
    public T? Value { get; }

    private EnvelopeTemplate(
        T value,
        string methodName,
        IEnumerable<string> errors,
        DateTime timeGenerated,
        int operationStatus
    )
        : base(methodName, errors, timeGenerated, operationStatus) { }

    private EnvelopeTemplate(T value, EnvelopeTemplate template)
        : base(template) { }

    private EnvelopeTemplate(EnvelopeTemplate template)
        : base(template) { }

    public static EnvelopeTemplate<T> FromResult(Result<T> result, string methodName)
    {
        EnvelopeTemplate template = EnvelopeTemplate.FromResult(result, methodName);
        return result.IsFailure
            ? new EnvelopeTemplate<T>(template)
            : new EnvelopeTemplate<T>(result.Value, template);
    }
}
