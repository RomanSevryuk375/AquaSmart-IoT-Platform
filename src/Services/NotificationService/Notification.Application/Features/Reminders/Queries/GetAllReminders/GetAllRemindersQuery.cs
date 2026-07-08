using Contracts.Abstractions;
using Contracts.Results;
using Notification.Application.Features.Reminders.Queries.Shared;

namespace Notification.Application.Features.Reminders.Queries.GetAllReminders;

public sealed record GetAllRemindersQuery
    : IQuery<Result<IReadOnlyList<ReminderDto>>>
{
    public Guid UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? LastDoneAtFrom { get; init; }
    public DateTime? LastDoneAtTo { get; init; }
    public DateTime? NextDueAtFrom { get; init; }
    public DateTime? NextDueAtTo { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
