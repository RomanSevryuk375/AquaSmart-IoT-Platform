using Contracts.Authorization;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.CreateRule;
using Control.Application.Features.AutomationRules.Commands.DeleteRule;
using Control.Application.Features.AutomationRules.Commands.UpdateRule;
using Control.Application.Features.AutomationRules.Queries;
using Control.Application.Features.AutomationRules.Queries.GetRuleById;
using Control.Application.Features.Ecosystems.Queries;
using Control.Application.Features.Ecosystems.Queries.GetAllEcosystems;
using Control.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/automation-rules")]
public class AutomationRulesController(
    IUserContext userContext,
    ISender sender) : ControllerBase
{
    private const string GetRuleByIdRoute = "GetRuleById";

    [HttpGet]
    [Authorize]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<IReadOnlyList<AutomationRuleDto>>> GetAllRulesAsync(
        [FromQuery] GetAllEcosystemsQuery query,
        CancellationToken cancellationToken = default)
    {
        GetAllEcosystemsQuery enrichedQuery = query with { UserId = userContext.UserId };
        IReadOnlyList<EcosystemDto> result = await sender.Send(enrichedQuery, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = GetRuleByIdRoute)]
    [Authorize]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<AutomationRuleDto>> GetRuleByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetRuleByIdQuery query = new GetRuleByIdQuery { RuleId = id };
        Result<AutomationRuleDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<Guid>> CreateRuleAsync(
        [FromBody] CreateRuleCommand command,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtRoute(
            GetRuleByIdRoute,
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> UpdateRuleAsync(
        [FromRoute] Guid id,
        [FromBody] AutomationRuleUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        UpdateRuleCommand command = new UpdateRuleCommand
        {
            RuleId = id,
            Name = request.Name,
            RelayId = request.RelayId,
            Operator = request.Operator,
            Action = request.Action,
        };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> DeleteRuleAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteRuleCommand command = new DeleteRuleCommand { RuleId = id };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}