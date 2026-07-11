// Ignore Spelling: Dto

namespace Device.Application.Features.Integrations.GetDeviceMetadata;

public sealed record DeviceMetadataDto
{
    public string? ControllerName { get; init; }
    public string? SensorName { get; init; }
    public string? RelayName { get; init; }
}
