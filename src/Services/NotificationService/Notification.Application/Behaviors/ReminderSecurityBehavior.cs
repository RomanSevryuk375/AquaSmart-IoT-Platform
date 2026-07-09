using System.Data;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;

namespace Notification.Application.Behaviors;

public sealed class ReminderSecurityBehavior<TRequest, TResponse>(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IReminderBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = "SELECT user_id FROM reminders WHERE id = @ReminderId LIMIT 1";

        Guid? ownerId = await connection.QuerySingleOrDefaultAsync<Guid?>(SQL, new { request.ReminderId });

        if (ownerId is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(
                Error.NotFound("Reminder.NotFound", $"Reminder {request.ReminderId} not found."));
        }

        if (ownerId != userContext.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(
                Error.Conflict("Access.Denied", "You are not the owner of this reminder."));
        }

        return await next();
    }
}
