using Device.Application.DTOs.Telemetry;

namespace Device.Application.Interfaces;

public interface ITelemtryBatchService
{
    Task<Result<TelemetryResponse>> ProcessTelemetryBatchAsync(
        TelemetryBatchRequest request,
        string deviceToken,
        CancellationToken cancellationToken);
}