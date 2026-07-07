using System.Data;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Behaviors;

public sealed class ScheduleSecurityBehavior<TRequest, TResponse>(
    ISqlConnectionFactory sqlConnectionFactory,
    IUserContext userContext)
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

        Guid? ownerId = await connection.QuerySingleOrDefaultAsync<Guid?>(SQL, new { request.ScheduleId });

        if (ownerId is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Schedule>(
                $"Schedule {request.ScheduleId} not found."));
        }

        if (ownerId != userContext.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Conflict("Access.Denied",
                "You are not the owner of this schedule."));
        }

        return await next();
    }
}
