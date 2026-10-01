namespace DTOs;

/// <summary>
/// Payload for creating a new reminder.
/// </summary>
public sealed record CreateReminderRequest
{
    public Guid EcosystemId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public int IntervalDays { get; init; }
}
