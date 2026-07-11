using Contracts.Abstractions;

namespace Device.Application.Features.Integrations.GetDeviceMetadata;

public sealed record GetDeviceMetadataQuery
    : IQuery<Result<DeviceMetadataDto>>
{
    public Guid? ControllerId { get; init; }
    public Guid? SensorId { get; init; }
    public Guid? RelayId { get; init; }
}
