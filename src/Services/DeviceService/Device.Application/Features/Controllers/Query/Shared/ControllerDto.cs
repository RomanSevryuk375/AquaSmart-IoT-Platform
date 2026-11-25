// Ignore Spelling: Dto

namespace Device.Application.Features.Controllers.Query.Shared;

public sealed record ControllerDto
{
    public Guid Id { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }
    public DateTime LastSeenAt { get; init; }
    public long ConfigVersion { get; init; }
    public string? FirmwareVersion { get; init; }
    public string? IpAddress { get; init; }
    public int? Rssi { get; init; }
    public long? UptimeSec { get; init; }
    public bool OfflineAlertSent { get; init; }
}
