using Contracts.Authorization;
using Contracts.Constants;
using Contracts.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;
using Notification.Application.Features.MaintenanceLogs.Queries.GetAllMaintenanceLogs;
using Notification.Application.Features.MaintenanceLogs.Queries.GetMaintenanceLogById;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;
using Notification.Domain.Interfaces;

namespace Notification.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.MaintenanceLogs)]
public class MaintenanceLogsController(
    ISender sender,
    IUserContext userContext) : ControllerBase
{
    private const string GetByIdRouteName = "GetMaintenanceLogById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.MaintenanceLogRead)]
    public async Task<ActionResult<IReadOnlyList<MaintenanceLogDto>>> GetAllLogsAsync(
        [FromQuery] Guid? ecosystemId,
        [FromQuery] DateTime? actionDateFrom,
        [FromQuery] DateTime? actionDateTo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllMaintenanceLogsQuery
        {
            UserId = userContext.UserId,
            EcosystemId = ecosystemId,
            ActionDateFrom = actionDateFrom,
            ActionDateTo = actionDateTo,
            Skip = skip,
            Take = take
        };

        Result<IReadOnlyList<MaintenanceLogDto>> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    [Authorize(Policy = SubPermissions.MaintenanceLogRead)]
    public async Task<ActionResult<MaintenanceLogDto>> GetLogByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMaintenanceLogByIdQuery
        {
            Id = id,
            UserId = userContext.UserId
        };

        Result<MaintenanceLogDto> result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.MaintenanceLogWrite)]
    public async Task<ActionResult<Guid>> AddLogAsync(
        [FromBody] CreateMaintenanceLogCommand command,
        CancellationToken cancellationToken = default)
    {
        CreateMaintenanceLogCommand enrichedCommand = command with { UserId = userContext.UserId };

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
}
