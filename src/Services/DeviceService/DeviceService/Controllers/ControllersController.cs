using Device.Application.DTOs.Configurations;
using Device.Application.DTOs.Controller;
using Device.Application.Features.Controllers.Command.PingController;

namespace Device.API.Controllers;

[ApiController]
[Route("api/device/v1/controllers")]
public sealed class ControllersController(
    IControllerService controllerService,
    IDeviceConfigurationService deviceConfigurationService) : ControllerBase
{
    private const string NameGetById = "GetControllerById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.DeviceControl)]
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

        return this.ToActionResult(result);
    }

    [HttpGet("me/config")]
    [AllowAnonymous]
    public async Task<ActionResult<ConfigResponseDto>> GetAllControllersAsync(
        [FromHeader(Name = "X-Mac-Address")] string macAddress,
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
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult<ControllerResponseDto>> GetControllerByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService.GetControllerByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> AddControllerAsync(
        [FromBody] ControllerRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService
            .AddControllerAsync(request, cancellationToken);

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
    public async Task<ActionResult<ControllerPingResponse>> PingControllerAsync(
        [FromRoute] Guid id,
        [FromHeader(Name = "X-Device-Token")] string deviceToken,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService
            .PingControllerAsync(id, deviceToken, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> UpdateControllerAsync(
        [FromRoute] Guid id,
        [FromBody] ControllerUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService.UpdateControllerAsync(id, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.DeviceControl)]
    public async Task<ActionResult> DeleteControllerAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await controllerService.DeleteControllerAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }
}
