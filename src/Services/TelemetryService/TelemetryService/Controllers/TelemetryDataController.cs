using Contracts.Authorization;
using Contracts.Constants;
using Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Application.Features.Telemetry.Queries.GetAggregatedTelemetryChart;
using Telemetry.Application.Features.Telemetry.Queries.GetRawTelemetryChart;

namespace Telemetry.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.Data)]
public class TelemetryDataController(ISender sender) : ControllerBase
{
    [HttpGet("raw")]
    [Authorize(Policy = SubPermissions.DataRealtime)]
    public async Task<ActionResult<TelemetryRawChartResponseDto>> GetAllRawDataAsync(
        [FromQuery] TelemetryDataFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRawTelemetryChartQuery
        {
            SensorId = filter.SensorId,
            From = filter.From,
            To = filter.To,
            Skip = skip,
            Take = take
        };

        Result<TelemetryRawChartResponseDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("aggregate")]
    [Authorize(Policy = SubPermissions.AnalyticsHistory)]
    public async Task<ActionResult<TelemetryChartResponseDto>> GetAllAggregatedDataAsync(
        [FromQuery] TelemetryAggregateFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAggregatedTelemetryChartQuery
        {
            SensorId = filter.SensorId,
            Period = filter.Period,
            From = filter.From,
            To = filter.To,
            Skip = skip,
            Take = take
        };

        Result<TelemetryChartResponseDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> ReceiveBatchTelemetryAsync(
        [FromBody] AddTelemetryBatchRequestDto request,
        [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = request.MacAddress,
            DeviceToken = deviceToken,
            Items = request.Items
        };

        Result result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return Accepted();
    }
}
