using Contracts.Authorization;
using Contracts.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.DTOs.MaintenanceLog;
using Notification.Application.Interfaces;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/notification/v1/maintenance-logs")]
public class MaintenanceLogsController(IMaintenanceLogService logService) : ControllerBase
{
    private const string GetByIdRouteName = "GetLogById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.MaintenanceLogRead)]
    public async Task<ActionResult<IReadOnlyList<MaintenanceLogResponseDto>>> GetAllLogsAsync(
        [FromQuery] MaintenanceLogFilterDto filter,
        [FromQuery] int? skip = 0,
        [FromQuery] int? take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await logService.GetAllLogs(filter, skip, take, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    [Authorize(Policy = SubPermissions.MaintenanceLogRead)]
    public async Task<ActionResult<MaintenanceLogResponseDto>> GetLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await logService.GetLogById(id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.MaintenanceLogWrite)]
    public async Task<ActionResult<Guid>> AddLogAsync(
        [FromBody] MaintenanceLogRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await logService.AddLogAsync(request, cancellationToken);

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