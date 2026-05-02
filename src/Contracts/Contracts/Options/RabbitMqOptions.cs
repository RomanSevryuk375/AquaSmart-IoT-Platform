namespace Contracts.Options;

public record RabbitMqOptions
{
    public const string SectionName = "RabbitMqOptions";
    public string Host { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
