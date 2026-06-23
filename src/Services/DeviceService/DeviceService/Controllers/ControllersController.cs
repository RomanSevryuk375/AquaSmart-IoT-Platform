using Device.Application.Features.Controllers.Command.AddController;
using Device.Application.Features.Controllers.Command.DeleteController;
using Device.Application.Features.Controllers.Command.PingController;
using Device.Application.Features.Controllers.Command.UpdateController;
using Device.Application.Features.Controllers.Query.GetAllControllers;
using Device.Application.Features.Controllers.Query.GetControllerById;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Application.Features.Controllers.Query.Shared;
using MediatR;

namespace Device.API.Controllers;

[ApiController]
[Route("api/device/v1/controllers")]
public sealed class ControllersController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    private const string NameGetById = "GetControllerById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<IReadOnlyList<ControllerDto>>> GetAllControllersAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isOnline,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllControllersQuery
        {
            UserId = userContext.UserId,
            SearchTerm = searchTerm,
            IsOnline = isOnline,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<ControllerDto>> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("me/config")]
    [AllowAnonymous]
    public async Task<ActionResult<ControllerConfig>> GetControllerConfigAsync(
        [FromHeader(Name = "X-Mac-Address")] string macAddress,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var query = new GetControllerConfigQuery
        {
            MacAddress = macAddress,
            DeviceToken = deviceToken
        };

        Result<ControllerConfig> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<ControllerDto>> GetControllerByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetControllerByIdQuery
        {
            UserId = userContext.UserId,
            ControllerId = id
        };

        Result<ControllerDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> AddControllerAsync(
        [FromBody] AddControllerCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<ControllerRegisteredResponse> result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            NameGetById,
            new { id = result.Value.ControllerId },
            result.Value);
    }

    [HttpPost("{id:guid}/ping")]
    [AllowAnonymous]
    public async Task<ActionResult> PingControllerAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var command = new PingControllerCommand
        {
            ControllerId = id,
            DeviceToken = deviceToken
        };

        Result<ControllerPingResponse> result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> UpdateControllerAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateControllerCommand command,
        CancellationToken cancellationToken = default)
    {
        UpdateControllerCommand enrichedCommand = command with { ControllerId = id };

        Result result = await sender.Send(enrichedCommand, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> DeleteControllerAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteControllerCommand { ControllerId = id };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
