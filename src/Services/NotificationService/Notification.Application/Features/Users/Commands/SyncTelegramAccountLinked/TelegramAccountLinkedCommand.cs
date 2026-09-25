using BuildingBlocks.Domain.Abstractions;

namespace Notification.Application.Features.Users.Commands.SyncTelegramAccountLinked;

public sealed record TelegramAccountLinkedCommand : ICommand
{
    public Guid UserId { get; init; }
    public long TelegramChatId { get; init; }
}
