using Contracts.Constants;

namespace Contracts.Results;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (ErrorInvalid(isSuccess, error))
        {
            throw new ArgumentException(ResulrErrors.InvalidErrorState, nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;

        static bool ErrorInvalid(bool isSuccess, Error error)
        {
            return isSuccess && error != Error.None || !isSuccess && error == Error.None;
        }
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(ResulrErrors.ResultIsFailure);

    private Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(value, true, Error.None);
    public static new Result<T> Failure(Error error) => new(default, false, error);

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}
