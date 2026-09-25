namespace BuildingBlocks.IntegrationEvents.Events.Users;

public sealed record TelegramAccountLinkedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public long TelegramChatId { get; init; }
}
