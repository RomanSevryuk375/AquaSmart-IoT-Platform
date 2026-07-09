using Contracts.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace IdentityService.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
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

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            ValidationFailure firstFailure = failures[0];
            var error = Error.Validation("Validation.Error", firstFailure.ErrorMessage);

            return BehaviorHelpers.CreateFailedResult<TResponse>(error);
        }

        return await next();
    }
}
