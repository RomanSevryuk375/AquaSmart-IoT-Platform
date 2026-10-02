using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results.Converters;

namespace BuildingBlocks.Domain.Results;

[JsonConverter(typeof(ResultJsonConverter))]
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    [JsonConstructor]
    protected Result(bool isSuccess, Error error)
    {
        if (ErrorInvalid(isSuccess, error))
        {
            throw new ArgumentException(ResultErrors.InvalidErrorState, nameof(error));
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

[JsonConverter(typeof(ResultJsonConverterFactory))]
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(ResultErrors.ResultIsFailure);

    [JsonConstructor]
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
