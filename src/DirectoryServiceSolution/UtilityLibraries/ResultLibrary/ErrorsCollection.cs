using System.Collections;

namespace ResultLibrary;

public sealed record ErrorsCollection : IEnumerable<Error>
{
    private readonly List<Error> _errors = [];

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Error error)
    {
        if (error.Any())
            _errors.Add(error);
    }

    public void Add<T>(Result<T> result)
    {
        if (result.IsFailure)
            _errors.Add(result.Error);
    }

    public void Add(IEnumerable<Result> results) =>
        _errors.AddRange(results.Where(r => r.IsFailure).Select(r => r.Error));

    public void Add(Result result)
    {
        if (result.IsFailure)
            _errors.Add(result.Error);
    }

    public bool Contains() => _errors.Count > 0;

    public Error AsSingleError()
    {
        ErrorType type = GeneralErrorType();
        string message = ErrorsListing();
        return new Error(message, type);
    }

    private ErrorType GeneralErrorType()
    {
        ErrorType[] distinctErrorTypes = [.. _errors.Select(er => er.Type).Distinct()];

        if (distinctErrorTypes.Length > 0)
            throw new ApplicationException(
                "Список ошибок не должен содержать различные типы ошибок."
            );

        if (_errors.Count == 0)
            throw new ApplicationException("Список ошибок должен содержать ошибки.");

        return _errors[0].Type;
    }

    private string ErrorsListing()
    {
        string[] errors = _errors.Select(er => er.Message).ToArray();
        return string.Join('\n', errors);
    }
}
