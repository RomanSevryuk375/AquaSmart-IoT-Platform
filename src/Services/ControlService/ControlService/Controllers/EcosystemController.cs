using Contracts.Authorization;
using Contracts.Results;
using Control.Application.CQRS.Ecosystem.Commands.CreateEcosystem;
using Control.Application.CQRS.Ecosystem.Commands.DeleteEcosystem;
using Control.Application.CQRS.Ecosystem.Commands.UpdateEcosystem;
using Control.Application.CQRS.Ecosystem.Queries;
using Control.Application.CQRS.Ecosystem.Queries.GetAllEcosystems;
using Control.Application.CQRS.Ecosystem.Queries.GetEcosystemById;
using Control.Application.DTOs.Ecosystem;
using Control.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/ecosystems")]
public sealed class EcosystemController(
    IUserContext userContext,
    ISender sender) : ControllerBase
{
    private const string GetByIdRoute = "GetEcosystemById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.TankRead)]
    public async Task<ActionResult<IReadOnlyList<EcosystemDto>>> GetAllEcosystemsAsync(
        [FromQuery] GetAllEcosystemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var enrichedQuery = query with { UserId = userContext.UserId };
        var result = await sender.Send(enrichedQuery, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [Authorize(Policy = SubPermissions.TankRead)]
    public async Task<ActionResult<EcosystemDto>> GetEcosystemByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetEcosystemByIdQuery { EcosystemId = id };
        var result = await sender.Send(query, cancellationToken);

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
        var command = new UpdateEcosystemCommand
        {
            EcosystemId = id,
            Name = request.Name,
            Volume = request.Volume
        };

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.TankDelete)]
    public async Task<IActionResult> DeleteEcosystemAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteEcosystemCommand { EcosystemId = id };
        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}