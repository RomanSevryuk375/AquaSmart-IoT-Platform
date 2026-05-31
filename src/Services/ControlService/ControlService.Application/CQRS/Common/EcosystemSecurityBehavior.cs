using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Common;

public class EcosystemSecurityBehavior<TRequest, TResponse>(ISecureService secureService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IEcosystemBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var hasAccess = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if(hasAccess.IsFailure)
        {
            var error = hasAccess.Error;

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

            throw new UnauthorizedAccessException($"Access failure: {error.Message}");
        }

        return await next();
    }
}
