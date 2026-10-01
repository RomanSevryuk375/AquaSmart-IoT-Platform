namespace DTOs;

/// <summary>
/// Reminder data returned from NotificationService.
/// </summary>
public sealed record ReminderDto
{
    public Guid Id { get; init; }
    public Guid EcosystemId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
    public DateTime? LastDoneAt { get; init; }
    public DateTime NextDueAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
