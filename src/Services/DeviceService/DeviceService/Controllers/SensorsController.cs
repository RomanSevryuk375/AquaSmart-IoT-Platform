using Contracts.Enums;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Features.Sensors.Command.DeleteSensor;
using Device.Application.Features.Sensors.Command.UpdateSensor;
using Device.Application.Features.Sensors.Query.GetAllSensors;
using Device.Application.Features.Sensors.Query.GetSensorById;
using Device.Application.Features.Sensors.Query.Shared;
using Device.Application.Features.Telemetry.Command.TransmittTelemetry;
using MediatR;

namespace Device.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.Sensors)]
public sealed class SensorsController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    private const string NameGetById = "GetSensorById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<IReadOnlyList<SensorDto>>> GetAllSensorsAsync(
        [FromQuery] Guid? controllerId,
        [FromQuery] SensorType? type,
        [FromQuery] SensorState? state,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllSensorsQuery
        {
            UserId = userContext.UserId,
            ControllerId = controllerId,
            Type = type,
            State = state,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<SensorDto>> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<SensorDto>> GetSensorByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSensorByIdQuery
        {
            UserId = userContext.UserId,
            SensorId = id
        };

        Result<SensorDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> AddSensorAsync(
        [FromBody] AddSensorCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<SensorCreatedResponse> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            NameGetById,
            new { id = result.Value.Id },
            result.Value);
    }

    [HttpPost("telemetry")]
    [AllowAnonymous]
    public async Task<ActionResult> ReceiveBatchTelemetryAsync(
        [FromBody] TransmitTelemetryCommand command,
        [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        TransmitTelemetryCommand enrichedCommand = command with { DeviceToken = deviceToken };

        Result<TelemetryTransmittedResponse> result = await sender.Send(enrichedCommand, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return Accepted(result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> UpdateSensorAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateSensorCommand command,
        CancellationToken cancellationToken = default)
    {
        UpdateSensorCommand enrichedCommand = command with { SensorId = id };

        Result result = await sender.Send(enrichedCommand, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> DeleteSensorAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSensorCommand { SensorId = id };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
