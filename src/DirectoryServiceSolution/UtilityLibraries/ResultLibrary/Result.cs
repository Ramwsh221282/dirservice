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

    public static Result Success()
    {
        return new Result();
    }

    public static Result Fail(string message, ErrorType errorType)
    {
        Error error = new(message, errorType);
        return Fail(error);
    }

    public static Result Fail(Error error)
    {
        return new Result(error);
    }

    public static implicit operator Result(Error error)
    {
        return Fail(error);
    }

    public static implicit operator Error(Result result)
    {
        return result.Error;
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue _value = default!;

    public TValue Value =>
        IsSuccess
            ? _value
            : throw new InvalidOperationException("Нельзя получить доступ к неуспешному результату.");

    private Result(TValue value)
        : base(true, false, new Error("", new NoErrorType()))
    {
        _value = value;
    }

    private Result(Error error)
        : base(error) { }

    private Result(string message, ErrorType errorType)
        : base(new Error(message, errorType)) { }

    private Result(Result other)
        : base(other.IsSuccess, other.IsFailure, other.Error) { }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static new Result<TValue> Fail(string message, ErrorType errorType)
    {
        return new Result<TValue>(new Error(message, errorType));
    }        

    public static new Result<TValue> Fail(Error error)
    {
        return new Result<TValue>(error);
    }

    public static implicit operator Result<TValue>(TValue value)
    {
        return Result<TValue>.Success(value);
    }

    public static implicit operator Result<TValue>(Error error)
    {
        return Result<TValue>.Fail(error);
    }

    public static implicit operator Error(Result<TValue> result)
    {
        return result.Error;
    }

    public static implicit operator TValue(Result<TValue> result)
    {
        return result.Value;
    }
}
