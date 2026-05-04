using Contracts.Authorization;
using Contracts.Results;
using Device.Application.DTOs.RelayCommands;
using Device.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.API.Controllers;

[ApiController]
[Route("api/device/v1/commands")]
public sealed class RelayCommandsController(
    IRelayCommandQueueService commandService) : ControllerBase
{
    [HttpGet("pending/{controllerId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<RelayCommandResponseDto>>> GetPendingCommands(
        [FromRoute] Guid controllerId,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken)
    {
        var result = await commandService
            .GetPendingCommands(controllerId, deviceToken, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{commandId:guid}/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkAsCompleted(
        [FromRoute] Guid commandId,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken)
    {
        var result = await commandService
            .MarkAsCompletedByIdAsync(commandId, deviceToken, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{commandId:guid}/fail")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkAsFailed(
        [FromRoute] Guid commandId,
        [FromBody] string errorMessage,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken)
    {
        var result = await commandService
            .MarkAsFailedByIdAsync(commandId, deviceToken, errorMessage, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("toggle-state/{relayId:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<bool>> ToggleRelayState(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var result = await commandService
            .ToggleRelayStateAsync(relayId, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("toggle-mode/{relayId:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<bool>> ToggleRelayMode(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var result = await commandService
            .ToggleRelayModeAsync(relayId, cancellationToken);

        return this.ToActionResult(result);
    }
}