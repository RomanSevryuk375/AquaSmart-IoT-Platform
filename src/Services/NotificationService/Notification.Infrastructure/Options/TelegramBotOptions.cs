namespace Notification.Infrastructure.Options;

public sealed record TelegramBotOptions
{
    public const string SectionName = "TelegramBotOptions";
    public string Name { get; init; } = string.Empty;
    public string BotSecretKey { get; init; } = string.Empty;
}
