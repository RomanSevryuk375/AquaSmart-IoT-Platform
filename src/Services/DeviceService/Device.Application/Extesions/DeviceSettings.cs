namespace Device.Application.Extesions;

public sealed class DeviceSettings
{
    public const string SectionName = "DeviceSettings";
    public int CommandTtlMinutes { get; init; } = 15;
    public int DefaultSendIntervalMs { get; init; } = 5000;
    public int MaxConfigBatchSize { get; init; } = 50;
}