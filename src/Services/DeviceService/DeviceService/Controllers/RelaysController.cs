using Device.Application.DTOs.Relay;
using Device.Application.Features.Relays.Command.AddRelay;

namespace Device.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.DeviceControl)]
[Route("api/device/v1/relays")]
public class RelaysController(
    IRelayService relayService) : ControllerBase
{
    private const string NameGetById = "GetRelayByIdAsync";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RelayCreatedResponse>>> GetAllRelaysAsync(
        [FromQuery] RelayFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.GetAllRelaysAsync(
            filter, 
            skip, 
            take, 
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    public async Task<ActionResult<RelayCreatedResponse>> GetRelayByIdAsync(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.GetRelayByIdAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddRelayAsync(
        [FromBody] RelayRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.AddRelayAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            NameGetById,
            new { id = result.Value.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateRelayAsync(
        [FromRoute] Guid id,
        [FromBody] RelayUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.UpdateRelayAsync(id, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRelayAsync(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.DeleteRelayAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }
}
