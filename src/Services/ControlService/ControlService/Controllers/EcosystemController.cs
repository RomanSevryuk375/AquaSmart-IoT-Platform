using Contracts.Authorization;
using Contracts.Results;
using Control.Application.CQRS.Ecosystem.Commands.CreateEcosystem;
using Control.Application.DTOs.Ecosystem;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/ecosystems")]
public class EcosystemController(
    IEcosystemService ecosystemService,
    IUserContext userContext,
    ISender sender) : ControllerBase
{
    private const string GetByIdRoute = "GetEcosystemById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.TankRead)]
    public async Task<ActionResult<IReadOnlyList<EcosystemResponseDto>>> GetAllEcosystemsAsync(
        [FromQuery] EcosystemFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await ecosystemService.GetAllEcosystemsAsync(
            filter, skip, take, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [Authorize(Policy = SubPermissions.TankRead)]
    public async Task<ActionResult<EcosystemResponseDto>> GetEcosystemByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await ecosystemService.GetEcosystemByIdAsync(
            id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.TankCreate)]
    public async Task<ActionResult<Guid>> CreateEcosystemAsync(
        [FromBody] EcosystemRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateEcosystemCommand
        {
            Type = request.Type,
            Name = request.Name,
            Volume = request.Volume,
            ControllerId = request.ControllerId,
            UserId = userContext.UserId
        };

        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            GetByIdRoute,
            new { id = result.Value }, 
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.TankUpdate)]
    public async Task<IActionResult> UpdateEcosystemAsync(
        [FromRoute] Guid id,
        [FromBody] EcosystemUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await ecosystemService.UpdateEcosystemAsync(
            id, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.TankDelete)]
    public async Task<IActionResult> DeleteEcosystemAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await ecosystemService.DeleteEcosystemAsync(
            id, cancellationToken);

        return this.ToActionResult(result);
    }
}