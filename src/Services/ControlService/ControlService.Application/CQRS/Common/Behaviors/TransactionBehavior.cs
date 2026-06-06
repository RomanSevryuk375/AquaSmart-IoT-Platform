using Contracts.Abstractions;
using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if(request is not IBaseCommand)
        {
            return await next();
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();

            if (response is Result { IsFailure: true })
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return response;
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
