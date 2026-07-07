using Control.Application.Features.Schedules.Commands.CreateSchedule;
using Control.Application.Features.Schedules.Commands.DeleteSchedule;
using Control.Application.Features.Schedules.Commands.SetIsActiveSchedule;
using Control.Application.Features.Schedules.Commands.UpdateSchedule;
using Control.Application.Features.Schedules.Queries.GetAllSchedules;
using Control.Application.Features.Schedules.Queries.GetScheduleById;
using Control.Application.Features.Schedules.Queries.Shared;

namespace Control.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.Schedules)]
public class SchedulesController(
    IUserContext userContext,
    ISender sender) : ControllerBase
{
    private const string GetScheduleByIdRoute = "GetScheduleById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<ActionResult<IReadOnlyList<ScheduleDto>>> GetAllSchedulesAsync(
        [FromQuery] GetAllSchedulesQuery query,
        CancellationToken cancellationToken = default)
    {
        GetAllSchedulesQuery enrichedQuery = query with { UserId = userContext.UserId };
        Result<IReadOnlyList<ScheduleDto>> result = await sender.Send(enrichedQuery, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetScheduleByIdRoute)]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<ActionResult<ScheduleDto>> GetScheduleByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetScheduleByIdQuery query = new GetScheduleByIdQuery
        {
            ScheduleId = id,
            UserId = userContext.UserId
        };
        Result<ScheduleDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<ActionResult<Guid>> CreateScheduleAsync(
        [FromBody] CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            GetScheduleByIdRoute,
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<IActionResult> UpdateScheduleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        UpdateScheduleCommand enrichedCommand = command with { ScheduleId = id };
        Result result = await sender.Send(enrichedCommand, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/active")]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<IActionResult> SetIsActiveScheduleAsync(
        [FromRoute] Guid id,
        [FromBody] SetIsActiveScheduleRequest request,
        CancellationToken cancellationToken)
    {
        SetIsActiveScheduleCommand command = new SetIsActiveScheduleCommand
        {
            ScheduleId = id,
            IsActive = request.IsActive
        };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.AutoScheduleCreate)]
    public async Task<IActionResult> DeleteScheduleAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteScheduleCommand command = new DeleteScheduleCommand { ScheduleId = id };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}

public record SetIsActiveScheduleRequest
{
    public required bool IsActive { get; init; }
}
