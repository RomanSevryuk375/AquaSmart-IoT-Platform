using Contracts.Authorization;
using Device.Application.DTOs.Relay;
using Device.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.DeviceControl)]
[Route("api/device/v1/relays")]
public class RelaysController(
    IRelayService relayService) : ControllerBase
{
    private const string NameGetById = "GetRelayByIdAsync";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RelayResponseDto>>> GetAllRelaysAsync(
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

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    public async Task<ActionResult<RelayResponseDto>> GetRelayByIdAsync(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken = default)
    {
        var result = await relayService.GetRelayByIdAsync(id, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddRelayAsync(
        [FromBody] RelayRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var id = await relayService.AddRelayAsync(request, cancellationToken);

        var createdData = await relayService.GetRelayByIdAsync(id, cancellationToken);

        return CreatedAtRoute(
            NameGetById, 
            new { id },
            createdData);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateRelayAsync(
        [FromRoute] Guid id,
        [FromBody] RelayUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await relayService.UpdateRelayAsync(id, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRelayAsync(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken = default)
    {
        await relayService.DeleteRelayAsync(id, cancellationToken);

        return NoContent();
    }
}
