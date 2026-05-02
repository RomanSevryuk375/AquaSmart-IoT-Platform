namespace Notification.Application.DTOs.Reminder;

public record ReminderRequestDto
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
}
