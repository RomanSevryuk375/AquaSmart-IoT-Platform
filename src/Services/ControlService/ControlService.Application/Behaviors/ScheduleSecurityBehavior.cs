// Ignore Spelling: Validator

using System.Data;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Dapper;
using MediatR;

namespace Control.Application.Behaviors;

public sealed class ScheduleSecurityBehavior<TRequest, TResponse>(
    ISqlConnectionFactory sqlConnectionFactory)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IScheduleBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT e.user_id 
            FROM schedules s
            JOIN ecosystems e ON s.ecosystem_id = e.id
            WHERE s.id = @ScheduleId
            LIMIT 1
            """;

        Guid? ownerId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(SQL, new { request.ScheduleId }, cancellationToken: cancellationToken));

        if (ownerId is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Schedule>(
                $"Schedule {request.ScheduleId} not found."));
        }

        if (ownerId != request.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Forbidden(
                ErrorCodes.Security.AccessDenied,
                ErrorMessages.Security.YouAreNotOwnerOfSchedule));
        }

        return await next(cancellationToken);
    }
}
