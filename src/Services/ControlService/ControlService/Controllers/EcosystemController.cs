using Contracts.Authorization;
using Contracts.Results;
using Control.Application.DTOs.Ecosystem;
using Control.Application.Features.Ecosystems.Commands.CreateEcosystem;
using Control.Application.Features.Ecosystems.Commands.DeleteEcosystem;
using Control.Application.Features.Ecosystems.Commands.UpdateEcosystem;
using Control.Application.Features.Ecosystems.Queries;
using Control.Application.Features.Ecosystems.Queries.GetAllEcosystems;
using Control.Application.Features.Ecosystems.Queries.GetEcosystemById;
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
        GetAllEcosystemsQuery enrichedQuery = query with { UserId = userContext.UserId };
        IReadOnlyList<EcosystemDto> result = await sender.Send(enrichedQuery, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = GetByIdRoute)]
    [Authorize(Policy = SubPermissions.TankRead)]
    public async Task<ActionResult<EcosystemDto>> GetEcosystemByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetEcosystemByIdQuery query = new GetEcosystemByIdQuery { EcosystemId = id };
        Result<EcosystemDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.TankCreate)]
    public async Task<ActionResult<Guid>> CreateEcosystemAsync(
        [FromBody] EcosystemRequestDto request,
        CancellationToken cancellationToken)
    {
        CreateEcosystemCommand command = new CreateEcosystemCommand
        {
            Type = request.Type,
            Name = request.Name,
            Volume = request.Volume,
            ControllerId = request.ControllerId,
            UserId = userContext.UserId
        };

        Result<Guid> result = await sender.Send(command, cancellationToken);
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
        UpdateEcosystemCommand command = new UpdateEcosystemCommand
        {
            EcosystemId = id,
            Name = request.Name,
            Volume = request.Volume
        };

        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.TankDelete)]
    public async Task<IActionResult> DeleteEcosystemAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteEcosystemCommand command = new DeleteEcosystemCommand { EcosystemId = id };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}