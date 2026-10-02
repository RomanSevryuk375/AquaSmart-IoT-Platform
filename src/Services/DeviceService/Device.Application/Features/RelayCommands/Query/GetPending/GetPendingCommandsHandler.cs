using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Dapper;

namespace Device.Application.Features.RelayCommands.Query.GetPending;

internal sealed class GetPendingCommandsHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetPendingCommandsQuery, Result<IReadOnlyList<RelayCommandDto>>>
{
    public async Task<Result<IReadOnlyList<RelayCommandDto>>> Handle(
        GetPendingCommandsQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        DateTime now = DateTime.UtcNow;
        DateTime retryThreshold = now.AddMinutes(-RelayCommandConstants.RetryCooldownMinutes);

        const string PopSql = """
            UPDATE relay_command_queues
            SET status = @SentStatus,
                attempt_count = attempt_count + 1,
                processed_at = @Now
            WHERE id IN (
                SELECT id FROM relay_command_queues
                WHERE controller_id = @ControllerId
                  AND (expire_at IS NULL OR expire_at > @Now)
                  AND (status = @PendingStatus OR
                      (status = @SentStatus
                        AND attempt_count < @MaxAttemptCount
                        AND processed_at < @RetryThreshold))
                ORDER BY created_at
                FOR UPDATE SKIP LOCKED)
            RETURNING
                id, controller_id, relay_id, 
                CASE WHEN target_state
                  THEN 1
                  ELSE 2
                END AS action,
                status, expire_at, attempt_count, processed_at, error_message, created_at;
            """;

        IEnumerable<RelayCommandDto> commands = await connection.QueryAsync<RelayCommandDto>(PopSql, new
        {
            request.ControllerId,
            Now = now,
            RetryThreshold = retryThreshold,
            MaxAttemptCount = RelayCommandConstants.MaxAttemptCount,
            SentStatus = (int)CommandStatus.Sent,
            PendingStatus = (int)CommandStatus.Pending
        });

        return Result<IReadOnlyList<RelayCommandDto>>.Success(
            commands.AsList().AsReadOnly());
    }
}
