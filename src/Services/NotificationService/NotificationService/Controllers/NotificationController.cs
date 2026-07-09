using Contracts.Constants;
using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using Notification.Application.Features.Notifications.Queries.GetAllNotifications;
using Notification.Application.Features.Notifications.Queries.GetNotificationById;
using Notification.Application.Features.Notifications.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.API.Controllers;

[ApiController]
[Authorize]
[Route(ApiConstants.Routes.Notifications)]
public class NotificationController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAllNotificationsAsync(
        [FromQuery] Guid? ecosystemId,
        [FromQuery] NotificationLevel? level,
        [FromQuery] bool? isRead,
        [FromQuery] string? searchTerm,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllNotificationsQuery
        {
            UserId = userContext.UserId,
            EcosystemId = ecosystemId,
            Level = level,
            IsRead = isRead,
            SearchTerm = searchTerm,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<NotificationDto>> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetNotificationByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationByIdQuery
        {
            NotificationId = id,
            UserId = userContext.UserId
        };

        Result<NotificationDto> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkNotificationAsReadAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = id,
            UserId = userContext.UserId
        };

        Result result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
