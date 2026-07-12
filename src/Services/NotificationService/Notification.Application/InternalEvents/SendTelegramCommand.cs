namespace Notification.Application.InternalEvents;

public sealed record SendTelegramCommand
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid NotificationId { get; init; }
    public long ChatId { get; init; }
    public string Message { get; init; } = string.Empty;
}
