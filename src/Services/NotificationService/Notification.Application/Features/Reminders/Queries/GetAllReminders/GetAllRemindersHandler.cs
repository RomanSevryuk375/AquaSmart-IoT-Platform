using System.Data;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.Reminders.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Queries.GetAllReminders;

public sealed class GetAllRemindersHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllRemindersQuery, Result<IReadOnlyList<ReminderDto>>>
{
    public async Task<Result<IReadOnlyList<ReminderDto>>> Handle(
        GetAllRemindersQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id AS Id, 
                user_id AS UserId, 
                ecosystem_id AS EcosystemId, 
                task_name AS TaskName, 
                interval_days AS IntervalDays, 
                last_done_at AS LastDoneAt, 
                last_notified_at AS LastNotifiedAt, 
                next_due_at AS NextDueAt, 
                created_at AS CreatedAt
            FROM reminders
            WHERE user_id = @UserId
              AND (@EcosystemId IS NULL OR ecosystem_id = @EcosystemId)
              AND (@SearchTerm IS NULL OR @SearchTerm = '' OR task_name ILIKE '%' || @SearchTerm || '%')
              AND (@LastDoneAtFrom IS NULL OR last_done_at >= @LastDoneAtFrom)
              AND (@LastDoneAtTo IS NULL OR last_done_at <= @LastDoneAtTo)
              AND (@NextDueAtFrom IS NULL OR next_due_at >= @NextDueAtFrom)
              AND (@NextDueAtTo IS NULL OR next_due_at <= @NextDueAtTo)
            ORDER BY next_due_at ASC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<ReminderDto> reminders = await connection.QueryAsync<ReminderDto>(SQL, request);

        return Result<IReadOnlyList<ReminderDto>>.Success(reminders.ToList().AsReadOnly());
    }
}
