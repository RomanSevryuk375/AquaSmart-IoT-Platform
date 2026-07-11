// Ignore Spelling: Enricher

using Contracts.Results;
using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

public interface IDeviceMetadataEnricher
{
    public Task<Result<DeviceMetadataDto>> EnrichAsync(
        Guid? controllerId,
        Guid? sensorId,
        Guid? relayId,
        CancellationToken cancellationToken = default);
}
