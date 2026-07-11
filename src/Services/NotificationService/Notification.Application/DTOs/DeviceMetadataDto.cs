// Ignore Spelling: Dto

namespace Notification.Application.DTOs;

public sealed record DeviceMetadataDto
{
    public string ControllerName { get; init; } = string.Empty;
    public string SensorName { get; init; } = string.Empty;
    public string RelayName { get; init; } = string.Empty;
}
