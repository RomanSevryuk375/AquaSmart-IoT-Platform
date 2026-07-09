using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface ITelemetryDataService
{
    public Task<ConsumerResult> AddDataAsync(
        TelemetryBatchEvent telemetry,
        CancellationToken cancellationToken);

    public Task<Result<TelemetryRawChartResponseDto>> GetAllDataAsync(
        TelemetryDataFilterDto filter,
        int skip,
        int take,
        CancellationToken cancellationToken);

}