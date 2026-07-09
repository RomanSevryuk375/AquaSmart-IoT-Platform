using Device.Application.Features.RelayCommands.Command.MarkAsCompleted;
using Device.Application.Features.RelayCommands.Command.MarkAsFailed;
using Device.Application.Features.RelayCommands.Command.ToggleRelayMode;
using Device.Application.Features.RelayCommands.Command.ToggleRelayState;
using Device.Application.Features.RelayCommands.Query.GetPending;
using MediatR;

namespace Device.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.Commands)]
public sealed class RelayCommandsController(ISender sender) : ControllerBase
{
    [HttpGet("pending/{controllerId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<RelayCommandDto>>> GetPendingCommandsAsync(
        [FromRoute] Guid controllerId,
        [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingCommandsQuery
        {
            ControllerId = controllerId,
            DeviceToken = deviceToken
        };

        Result<IReadOnlyList<RelayCommandDto>> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{commandId:guid}/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkAsCompletedAsync(
        [FromRoute] Guid commandId,
        [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkAsCompletedCommand
        {
            CommandId = commandId,
            DeviceToken = deviceToken
        };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{commandId:guid}/fail")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkAsFailedAsync(
        [FromRoute] Guid commandId,
        [FromBody] string errorMessage,
        [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkAsFailedCommand
        {
            CommandId = commandId,
            DeviceToken = deviceToken,
            ErrorMessage = errorMessage
        };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("toggle-state/{relayId:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<bool>> ToggleRelayStateAsync(
        [FromRoute] Guid relayId,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleRelayStateCommand
        {
            RelayId = relayId
        };

        Result<bool> result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("toggle-mode/{relayId:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<bool>> ToggleRelayModeAsync(
        [FromRoute] Guid relayId,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleRelayModeCommand
        {
            RelayId = relayId
        };

        Result<bool> result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
