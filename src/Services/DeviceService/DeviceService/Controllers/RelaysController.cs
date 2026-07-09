using Contracts.Enums;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Application.Features.Relays.Command.DeleteRelay;
using Device.Application.Features.Relays.Command.UpdateRelay;
using Device.Application.Features.Relays.Query.GetAllRelays;
using Device.Application.Features.Relays.Query.GetRelayById;
using Device.Application.Features.Relays.Query.Shared;
using MediatR;

namespace Device.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.DeviceControl)]
[Route(ApiConstants.Routes.Relays)]
public sealed class RelaysController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    private const string NameGetById = "GetRelayById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RelayDto>>> GetAllRelaysAsync(
        [FromQuery] Guid? controllerId,
        [FromQuery] RelayPurpose? purpose,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isManual,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllRelaysQuery
        {
            UserId = userContext.UserId,
            ControllerId = controllerId,
            Purpose = purpose,
            IsActive = isActive,
            IsManual = isManual,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<RelayDto>> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    public async Task<ActionResult<RelayDto>> GetRelayByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRelayByIdQuery
        {
            UserId = userContext.UserId,
            RelayId = id
        };

        Result<RelayDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> AddRelayAsync(
        [FromBody] AddRelayCommand command,
        CancellationToken cancellationToken = default)
    {
        Result<RelayCreatedResponse> result = await sender.Send(command, cancellationToken);
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
        [FromBody] UpdateRelayCommand command,
        CancellationToken cancellationToken = default)
    {
        UpdateRelayCommand enrichedCommand = command with { RelayId = id };

        Result result = await sender.Send(enrichedCommand, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRelayAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteRelayCommand { RelayId = id };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
