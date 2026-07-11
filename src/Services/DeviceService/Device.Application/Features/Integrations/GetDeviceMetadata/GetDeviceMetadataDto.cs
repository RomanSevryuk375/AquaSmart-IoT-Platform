// Ignore Spelling: Dto

namespace Device.Application.Features.Integrations.GetDeviceMetadata;

public sealed record DeviceMetadataDto
{
    public string SensorName { get; init; } = string.Empty;
    public string RelayName { get; init; } = string.Empty;
}
