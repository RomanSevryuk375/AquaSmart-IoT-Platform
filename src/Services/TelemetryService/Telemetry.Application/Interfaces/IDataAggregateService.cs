using Contracts.Results;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Interfaces;

public interface IDataAggregateService
{
    public Task<Result<TelemetryChartResponseDto>> GetChartDataAsync(
        TelemetryAggregateFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);
}