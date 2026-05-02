using Contracts.Authorization;
using Contracts.Results;
using Device.Application.DTOs.Configurations;
using Device.Application.DTOs.Controller;
using Device.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.DeviceControl)]
[Route("api/device/v1/controllers")]
public class ControllersController(
    IControllerService controllerService,
    IDeviceConfigurationService deviceConfigurationService) : ControllerBase
{
    private const string NameGetById = "GetControllerById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ControllerResponseDto>>> GetAllControllersAsync(
        [FromQuery] ControllerFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService.GetAllControllersAsync(
            filter,
            skip,
            take,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("me/config")]
    public async Task<ActionResult<ConfigResponseDto>> GetAllControllersAsync(
        [FromBody] string macAddress,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var result = await deviceConfigurationService.GetControllerConfigAsync(
            macAddress,
            deviceToken,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    public async Task<ActionResult<ControllerResponseDto>> GetControllerByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService.GetControllerByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddControllerAsync(
        [FromBody] ControllerRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await controllerService
            .AddControllerAsync(request, cancellationToken);

        return CreatedAtRoute(
            NameGetById,
            new { id = response.ControllerId },
            response);
    }

    [HttpPost("{id:guid}/ping")]
    [AllowAnonymous]
    public async Task<ActionResult<ControllerPingResponseDto>> PingControllerAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService
            .PingControllerAsync(id, deviceToken, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateControllerAsync(
        [FromRoute] Guid id,
        [FromBody] ControllerUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await controllerService.UpdateControllerAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteControllerAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await controllerService.DeleteControllerAsync(id, cancellationToken);

        return NoContent();
    }
}
