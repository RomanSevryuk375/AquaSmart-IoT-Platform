namespace IdentityService.Application.Options;

public sealed record TelegramBotOptions
{
    public const string SectionName = "TelegramBotOptions";
    public string Name { get; set; } = string.Empty;
    public string BotSecretKey { get; set; } = string.Empty;
}
