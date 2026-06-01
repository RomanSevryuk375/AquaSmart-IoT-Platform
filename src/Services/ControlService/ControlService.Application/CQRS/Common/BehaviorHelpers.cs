using Contracts.Results;

namespace Control.Application.CQRS.Common;

public static class BehaviorHelpers
{
    public static TResponse CreateFailedResult<TResponse>(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultTypeArg = typeof(TResponse).GetGenericArguments()[0];
            var genericResultType = typeof(Result<>).MakeGenericType(resultTypeArg);
            var failureMethod = genericResultType.GetMethod("Failure");

            if (failureMethod != null)
            {
                var failedResult = failureMethod.Invoke(null, [error]);
                return (TResponse)failedResult!;
            }
        }

        throw new UnauthorizedAccessException($"Accsse failure: {error.Message}");
    }
}
