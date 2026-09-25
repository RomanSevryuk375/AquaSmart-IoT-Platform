using BuildingBlocks.Domain.Abstractions;

namespace IdentityService.Domain.Events;

public sealed record TelegramAccountLinkedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public long TelegramChatId { get; init; }
}
