namespace ResultLibrary;

public class Result
{
    public virtual bool IsSuccess { get; }
    public virtual bool IsFailure { get; }
    public virtual Error Error { get; }

    protected Result(Error error)
    {
        Error = error;
        IsSuccess = false;
        IsFailure = true;
    }

    protected Result()
    {
        Error = new Error("", new NoErrorType());
        IsSuccess = true;
        IsFailure = false;
    }

    protected Result(bool isSuccess, bool isFailure, Error error)
    {
        IsSuccess = isSuccess;
        IsFailure = isFailure;
        Error = error;
    }

    public static Result Success() => new Result();

    public static Result Fail(string message, ErrorType errorType)
    {
        Error error = new Error(message, errorType);
        return Fail(error);
    }

    public static Result Fail(Error error) => new Result(error);

    public static implicit operator Result(Error error) => Result.Fail(error);

    public static implicit operator Error(Result result) => result.Error;
}

public sealed class Result<TValue> : Result
{
    public TValue Value { get; } = default!;

    private Result(TValue value)
        : base(true, false, new Error("", new NoErrorType())) { }

    private Result(Error error)
        : base(error) { }

    private Result(string message, ErrorType errorType)
        : base(new Error(message, errorType)) { }

    private Result(Result other)
        : base(other.IsSuccess, other.IsFailure, other.Error) { }

    public static Result<TValue> Success(TValue value) => new Result<TValue>(value);

    public static new Result<TValue> Fail(string message, ErrorType errorType) =>
        new Result<TValue>(new Error(message, errorType));

    public static new Result<TValue> Fail(Error error) => new Result<TValue>(error);

    public static implicit operator Result<TValue>(TValue value) => Result<TValue>.Success(value);

    public static implicit operator Result<TValue>(Error error) => Result<TValue>.Fail(error);

    public static implicit operator Error(Result<TValue> result) => result.Error;

    public static implicit operator TValue(Result<TValue> result) => result.Value;
}
