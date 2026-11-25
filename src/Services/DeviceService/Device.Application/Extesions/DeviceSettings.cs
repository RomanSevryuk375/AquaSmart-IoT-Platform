// Ignore Spelling: Ttl

namespace Device.Application.Extesions;

public sealed record DeviceSettings
{
    public const string SectionName = "DeviceSettings";
    public int CommandTtlMinutes { get; init; } = 15;
    public int DefaultSendIntervalMs { get; init; } = 5000;
    public int MaxConfigBatchSize { get; init; } = 50;
    public int OfflineAlertGraceSeconds { get; init; } = 120;
    public int CommandRetryCooldownSeconds { get; init; } = 60;
    public int OnlineRedeliveryDelayMs { get; init; } = 1500;
    public string TokenHmacSecret { get; init; } = string.Empty;
}
