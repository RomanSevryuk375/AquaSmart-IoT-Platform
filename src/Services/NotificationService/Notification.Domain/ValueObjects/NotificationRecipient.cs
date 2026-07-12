// Ignore Spelling: Tg

namespace Notification.Domain.ValueObjects;

public sealed record NotificationRecipient
{
    public long? TgChatId { get; private set; }
    public string? Email { get; private set; }

    private NotificationRecipient(long? tgChatId, string? email)
    {
        TgChatId = tgChatId;
        Email = email;
    }

    public static NotificationRecipient TelegramRecipient(long tgChatId) => new(tgChatId, email: null);
    public static NotificationRecipient EmailRecipient(string? email) => new(tgChatId: null, email);
}
