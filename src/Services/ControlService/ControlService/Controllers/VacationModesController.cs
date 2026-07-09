using Control.Application.Features.VacationModes.Commands.CreateVacationMode;
using Control.Application.Features.VacationModes.Commands.DeleteVacationMode;
using Control.Application.Features.VacationModes.Commands.ToggleVacationMode;
using Control.Application.Features.VacationModes.Commands.UpdateVacationMode;
using Control.Application.Features.VacationModes.Queries.GetAllVacationModes;
using Control.Application.Features.VacationModes.Queries.GetVacationModeById;
using Control.Application.Features.VacationModes.Queries.Shared;

namespace Control.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.VacationModes)]
public class VacationModesController(
    IUserContext userContext,
    ISender sender) : ControllerBase
{
    private const string GetVacationModeByIdRoute = "GetVacationModeById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<ActionResult<IReadOnlyList<VacationModeDto>>> GetAllVacationModesAsync(
        [FromQuery] GetAllVacationModesQuery query,
        CancellationToken cancellationToken = default)
    {
        GetAllVacationModesQuery enrichedQuery = query with { UserId = userContext.UserId };
        Result<IReadOnlyList<VacationModeDto>> result = await sender.Send(enrichedQuery, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetVacationModeByIdRoute)]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<ActionResult<VacationModeDto>> GetVacationModeByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetVacationModeByIdQuery query = new GetVacationModeByIdQuery
        {
            VacationModeId = id,
            UserId = userContext.UserId
        };
        Result<VacationModeDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<ActionResult<Guid>> CreateVacationModeAsync(
        [FromBody] CreateVacationModeCommand command,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            GetVacationModeByIdRoute,
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<IActionResult> UpdateVacationModeAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateVacationModeCommand command,
        CancellationToken cancellationToken)
    {
        UpdateVacationModeCommand enrichedCommand = command with { VacationModeId = id };
        Result result = await sender.Send(enrichedCommand, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/toggle")]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<IActionResult> ToggleVacationModeAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        ToggleVacationModeCommand command = new ToggleVacationModeCommand { VacationModeId = id };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.VacationMode)]
    public async Task<IActionResult> DeleteVacationModeAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteVacationModeCommand command = new DeleteVacationModeCommand { VacationModeId = id };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}
