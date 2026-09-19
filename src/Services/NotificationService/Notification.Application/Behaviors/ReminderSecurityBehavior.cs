using System.Data;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Dapper;
using MediatR;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Application.Behaviors;

public sealed class ReminderSecurityBehavior<TRequest, TResponse>(
    ISqlConnectionFactory sqlConnectionFactory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IReminderBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string Sql = "SELECT user_id FROM reminders WHERE id = @ReminderId LIMIT 1";

        Guid? ownerId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(Sql, new { request.ReminderId }, cancellationToken: cancellationToken));

        if (ownerId is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Reminder>(
                    string.Format(ErrorMessages.Reminder.NotFoundFormat, request.ReminderId)));
        }

        if (ownerId != request.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Forbidden(
                ErrorCodes.Security.AccessDenied,
                ErrorMessages.Security.YouAreNotOwnerOfReminder));
        }

        return await next(cancellationToken);
    }
}
