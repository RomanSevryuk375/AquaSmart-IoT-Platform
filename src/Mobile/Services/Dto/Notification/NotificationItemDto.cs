namespace Mobile.Services.Dto.Notification;

public sealed record NotificationItemDto
{
    public Guid Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public int Level { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
