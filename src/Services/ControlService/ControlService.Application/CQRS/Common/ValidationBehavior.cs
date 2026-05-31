using Contracts.Results;
using FluentValidation;
using MediatR;

namespace Control.Application.CQRS.Common;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var firstFailure = failures[0];
            var error = Error.Validation("Validation.Error", firstFailure.ErrorMessage);

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

            throw new ValidationException(failures);
        }

        return await next();
    }
}
