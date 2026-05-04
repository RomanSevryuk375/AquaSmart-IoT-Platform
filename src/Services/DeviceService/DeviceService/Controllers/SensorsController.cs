using Contracts.Authorization;
using Contracts.Results;
using Device.Application.DTOs.Sensor;
using Device.Application.DTOs.Telemetry;
using Device.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.API.Controllers;

[ApiController]
[Route("api/device/v1/sensors")]
public class SensorsController(
    ISensorService sensorService,
    ITelemtryBatchService batchService) : ControllerBase
{
    private const string NameGetById = "GetSensorByIdAsync";

    [HttpGet]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<IReadOnlyList<SensorResponseDto>>> GetAllSensorsAsync(
        [FromQuery] SensorFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.GetAllSensorsAsync(
            filter,
            skip, 
            take, 
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<SensorResponseDto>> GetSensorByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.GetSensorByIdAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> AddSensorAsync(
        [FromBody] SensorRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.AddSensorAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        var createdData = await sensorService.GetSensorByIdAsync(result.Value, cancellationToken);

        return CreatedAtRoute(
            NameGetById,
            new { id = result.Value },
            createdData.Value);
    }

    [HttpPost("telemetry")]
    [AllowAnonymous]
    public async Task<ActionResult> ReceiveBatchTelemetryAsync(
    [FromBody] TelemetryBatchRequest request,
    [FromHeader(Name = "X-Device-Token")] string deviceToken,
    CancellationToken cancellationToken)
    {
        var result = await batchService
            .ProcessTelemetryBatchAsync(request, deviceToken, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return Accepted(new
        {
            result.Value.AcceptedCount,
            result.Value.ValidationErrors,
            result.Value.SkippedCount
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> UpdateSensorAsync(
        [FromRoute] Guid id,
        [FromBody] SensorUpdateRequestDto reuqest,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.UpdateSensorAsync(id, reuqest, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> DeleteSensorAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.DeleteSensorAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }
}
