using BuildingBlocks.Domain.Abstractions;

namespace Device.Application.Features.Mqtt;

public sealed record ProcessDeviceStatusCommand : ICommand
{
    public string TopicMac { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? FirmwareVersion { get; init; }
    public string? IpAddress { get; init; }
    public int? Rssi { get; init; }
    public long? UptimeSec { get; init; }
    public DateTime? Timestamp { get; init; }
}
