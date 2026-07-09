using Contracts.Authorization;
using Contracts.Constants;
using Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Features.Reminders.Commands.CompleteReminder;
using Notification.Application.Features.Reminders.Commands.CreateReminder;
using Notification.Application.Features.Reminders.Commands.DeleteReminder;
using Notification.Application.Features.Reminders.Commands.UpdateReminder;
using Notification.Application.Features.Reminders.Queries.GetAllReminders;
using Notification.Application.Features.Reminders.Queries.GetReminderById;
using Notification.Application.Features.Reminders.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.ReminderManage)]
[Route(ApiConstants.Routes.Reminders)]
public class RemindersController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    private const string GetByIdRouteName = "GetReminderById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderDto>>> GetAllAsync(
        [FromQuery] Guid? ecosystemId,
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? nextDueAtFrom,
        [FromQuery] DateTime? nextDueAtTo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllRemindersQuery
        {
            UserId = userContext.UserId,
            EcosystemId = ecosystemId,
            SearchTerm = searchTerm,
            NextDueAtFrom = nextDueAtFrom,
            NextDueAtTo = nextDueAtTo,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<ReminderDto>> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    public async Task<ActionResult<ReminderDto>> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReminderByIdQuery
        {
            ReminderId = id,
            UserId = userContext.UserId
        };

        Result<ReminderDto> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        [FromBody] CreateReminderCommand command,
        CancellationToken cancellationToken = default)
    {
        CreateReminderCommand enrichedCommand = command with { UserId = userContext.UserId };

        Result<Guid> result = await sender.Send(enrichedCommand, cancellationToken);

        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            GetByIdRouteName,
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateReminderCommand command,
        CancellationToken cancellationToken = default)
    {
        UpdateReminderCommand enrichedCommand = command with { ReminderId = id };

        Result result = await sender.Send(enrichedCommand, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTaskAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompleteReminderCommand { ReminderId = id };

        Result result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteReminderCommand { ReminderId = id };

        Result result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
