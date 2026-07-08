// Ignore Spelling: Dto

namespace Notification.Application.Features.Reminders.Queries.Shared;

public sealed record ReminderDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
    public DateTime? LastDoneAt { get; init; }
    public DateTime? LastNotifiedAt { get; init; }
    public DateTime NextDueAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
