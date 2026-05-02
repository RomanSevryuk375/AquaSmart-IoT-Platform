using Contracts.Authorization;
using Contracts.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;

namespace Telemetry.API.Controllers;

[ApiController]
[Route("api/telemetry/v1/data")]
public class TelemetryDataController(
    ITelemetryDataService rawService,
    IDataAggregateService aggregateService) : ControllerBase
{
    [HttpGet("raw")]
    [Authorize(Policy = SubPermissions.AccountView)]
    public async Task<ActionResult<TelemetryRawChartResponseDto>> GetAllRawDataAsync(
        [FromQuery] TelemetryDataFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await rawService.GetAllDataAsync(filter, skip, take, cancellationToken);
        
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
        var result = await aggregateService.GetChartDataAsync(filter, skip, take, cancellationToken);

        return this.ToActionResult(result);
    }
}
