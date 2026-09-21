// Ignore Spelling: Retryable

using System.Text.Json.Serialization;

namespace BuildingBlocks.Domain.Results;

public class ConsumerResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsRetryable { get; }

    [JsonConstructor]
    protected ConsumerResult(bool success, string? error, bool retryable)
    {
        IsSuccess = success;
        Error = error;
        IsRetryable = retryable;
    }

    public static ConsumerResult Success() => new(true, null, false);

    public static ConsumerResult FatalError(string message) => new(false, message, false);

    public static ConsumerResult RetryableError(string message) => new(false, message, true);
}
