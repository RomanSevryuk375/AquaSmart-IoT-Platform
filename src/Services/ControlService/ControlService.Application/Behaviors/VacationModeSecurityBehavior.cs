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

public sealed class VacationModeSecurityBehavior<TRequest, TResponse>(
    ISqlConnectionFactory sqlConnectionFactory)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IVacationModeBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT e.user_id 
            FROM vacations v
            JOIN ecosystems e ON v.ecosystem_id = e.id
            WHERE v.id = @VacationModeId
            LIMIT 1
            """;

        Guid? ownerId = await connection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(SQL, new { request.VacationModeId }, cancellationToken: cancellationToken));

        if (ownerId is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<VacationMode>(
                $"Vacation mode {request.VacationModeId} not found."));
        }

        if (ownerId != request.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Forbidden(
                ErrorCodes.Security.AccessDenied,
                ErrorMessages.Security.YouAreNotOwnerOfVacationMode));
        }

        return await next(cancellationToken);
    }
}
