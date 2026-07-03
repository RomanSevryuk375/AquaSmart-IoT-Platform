using System.Reflection;
using Contracts.Results;

namespace Control.Application.Features.Common;

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
            Type resultTypeArg = typeof(TResponse).GetGenericArguments()[0];
            Type genericResultType = typeof(Result<>).MakeGenericType(resultTypeArg);
            MethodInfo? failureMethod = genericResultType.GetMethod("Failure");

            if (failureMethod != null)
            {
                object? failedResult = failureMethod.Invoke(null, [error]);
                return (TResponse)failedResult!;
            }
        }

        throw new UnauthorizedAccessException($"Accsse failure: {error.Message}");
    }
}
