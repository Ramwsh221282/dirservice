using System.Collections;

namespace ResultLibrary;

public sealed class ErrorsCollection : Result, IEnumerable<Error>
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

    public override bool IsFailure => Contains();

    public override bool IsSuccess => !IsFailure;

    public override Error Error => AsSingleError();

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

    public ErrorType GeneralErrorType()
    {
        ErrorType[] distinctErrorTypes = [.. _errors.Select(er => er.Type).Distinct()];

        if (_errors.Count == 0)
            throw new ApplicationException("Список ошибок должен содержать ошибки.");

        if (distinctErrorTypes.Length > 0)
            return _errors[0].Type;

        throw new ApplicationException("Список ошибок не должен содержать различные типы ошибок.");
    }

    public IEnumerable<string> ErrorStrings()
    {
        return _errors.Select(er => er.Message);
    }

    private string ErrorsListing()
    {
        string[] errors = _errors.Select(er => er.Message).ToArray();
        return string.Join('\n', errors);
    }
}
