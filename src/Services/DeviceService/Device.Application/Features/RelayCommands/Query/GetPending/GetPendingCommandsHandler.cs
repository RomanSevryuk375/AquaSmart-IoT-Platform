using System.Data;
using Dapper;
using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Query.GetPending;

internal sealed class GetPendingCommandsHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IMyHasher hasher)
    : IRequestHandler<GetPendingCommandsQuery, Result<IReadOnlyList<RelayCommandDto>>>
{
    private const int MaxAttemptCount = 3;
    private const int RetryCooldownMinutes = 1;

    public async Task<Result<IReadOnlyList<RelayCommandDto>>> Handle(
        GetPendingCommandsQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string AuthSql = """
            SELECT device_token_hash
            FROM controllers
            WHERE id = @ControllerId
            LIMIT 1
            """;

        string? tokenHash = await connection.QueryFirstOrDefaultAsync<string>(
            AuthSql, new { request.ControllerId });
        if (tokenHash is null || !hasher.Verify(request.DeviceToken, tokenHash))
        {
            return Result<IReadOnlyList<RelayCommandDto>>.Failure(Error.NotFound<Controller>(
                "Invalid credentials or controller not found."));
        }

        DateTime now = DateTime.UtcNow;
        DateTime retryThreshold = now.AddMinutes(-RetryCooldownMinutes);

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
                id, controller_id, relay_id, action, status, 
                expire_at, attempt_count, processed_at, error_message, created_at;
            """;

        IEnumerable<RelayCommandDto> commands = await connection.QueryAsync<RelayCommandDto>(PopSql, new
        {
            request.ControllerId,
            Now = now,
            RetryThreshold = retryThreshold,
            MaxAttemptCount,
            SentStatus = (int)CommandStatus.Sent,
            PendingStatus = (int)CommandStatus.Pending
        });

        return Result<IReadOnlyList<RelayCommandDto>>.Success(
            commands.AsList().AsReadOnly());
    }
}
