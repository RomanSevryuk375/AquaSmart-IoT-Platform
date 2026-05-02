namespace Contracts.Options;

public record TelegramOptions
{
    public const string SectionName = "TelegramOptions";
    public string BotToken { get; init; } = string.Empty;
}
