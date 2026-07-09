using Contracts.Abstractions;
using Contracts.Results;
using Notification.Application.Features.Reminders.Queries.Shared;

namespace Notification.Application.Features.Reminders.Queries.GetReminderById;

public sealed record GetReminderByIdQuery
    : IQuery<Result<ReminderDto>>
{
    public Guid ReminderId { get; init; }
    public Guid UserId { get; init; }
}
