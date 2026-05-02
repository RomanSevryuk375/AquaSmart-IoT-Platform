using Contracts.Authorization;
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

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<SensorResponseDto>> GetSensorByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await sensorService.GetSensorByIdAsync(id, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> AddSensorAsync(
        [FromBody] SensorRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var id = await sensorService.AddSensorAsync(request, cancellationToken);

        var createdData = await sensorService.GetSensorByIdAsync(id, cancellationToken);

        return CreatedAtRoute(
            NameGetById,
            new { id },
            createdData);
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

        return Accepted(new
        {
            result.AcceptedCount,
            result.ValidationErrors,
            result.SkippedCount
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> UpdateSensorAsync(
        [FromRoute] Guid id,
        [FromBody] SensorUpdateRequestDto reuqest,
        CancellationToken cancellationToken = default)
    {
        await sensorService.UpdateSensorAsync(id, reuqest, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> DeleteSensorAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await sensorService.DeleteSensorAsync(id, cancellationToken);

        return NoContent();
    }
}
