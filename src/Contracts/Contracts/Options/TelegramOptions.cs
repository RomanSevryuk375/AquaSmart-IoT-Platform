namespace Contracts.Options;

public sealed record TelegramOptions
{
    public const string SectionName = "TelegramOptions";
    public string BotToken { get; init; } = string.Empty;
}
