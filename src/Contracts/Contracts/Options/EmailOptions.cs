namespace Contracts.Options;

public record EmailOptions
{
    public const string SectionName = "EmailOptions";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
}
