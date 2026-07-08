using System.Data;
using Contracts.Results;
using Dapper;
using MediatR;
using Notification.Application.Features.Reminders.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.Application.Features.Reminders.Queries.GetReminderById;

public sealed class GetReminderByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetReminderByIdQuery, Result<ReminderDto>>
{
    public async Task<Result<ReminderDto>> Handle(GetReminderByIdQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id AS Id, user_id AS UserId, ecosystem_id AS EcosystemId, 
                task_name AS TaskName, interval_days AS IntervalDays, 
                last_done_at AS LastDoneAt, last_notified_at AS LastNotifiedAt, 
                next_due_at AS NextDueAt, created_at AS CreatedAt
            FROM reminders
            WHERE id = @ReminderId 
              AND user_id = @UserId
            LIMIT 1
            """;

        ReminderDto? reminder = await connection.QuerySingleOrDefaultAsync<ReminderDto>(SQL, request);

        if (reminder is null)
        {
            return Result<ReminderDto>.Failure(Error.NotFound(
                "Reminder.NotFound", $"Reminder {request.ReminderId} not found."));
        }

        return Result<ReminderDto>.Success(reminder);
    }
}
