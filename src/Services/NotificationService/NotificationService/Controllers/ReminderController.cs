using Contracts.Authorization;
using Contracts.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.DTOs.Reminder;
using Notification.Application.Interfaces;

namespace Notification.API.Controllers;

[ApiController]
[Authorize(Policy = SubPermissions.ReminderManage)]
[Route("api/notification/v1/reminders")]
public class RemindersController(IReminderService reminderService) : ControllerBase
{
    private const string GetByIdRouteName = "GetReminderById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderResponseDto>>> GetAllAsync(
        [FromQuery] ReminderFilterDto filter,
        [FromQuery] int? skip = 0,
        [FromQuery] int? take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService
            .GetAllRemindersAsync(filter, skip, take, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    public async Task<ActionResult<ReminderResponseDto>> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService.GetReminderByIdAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        [FromBody] ReminderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService.AddReminderAsync(request, cancellationToken);

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
        [FromBody] ReminderUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService.UpdateReminderAsync(id, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTaskAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService.ReminderCompleteTaskAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await reminderService.DeleteReminderAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }
}