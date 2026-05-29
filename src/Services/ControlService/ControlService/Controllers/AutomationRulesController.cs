using Contracts.Authorization;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/automation-rules")]
public class AutomationRulesController(
    IAutomationRuleService ruleService) : ControllerBase
{
    private const string GetRuleByIdRoute = "GetRuleById";

    [HttpGet]
    [Authorize]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<IReadOnlyList<AutomationRuleResponseDto>>> GetAllRulesAsync(
        [FromQuery] AutomationRuleFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await ruleService.GetAllRulesAsync(
            filter, skip, take, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = GetRuleByIdRoute)]
    [Authorize]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<AutomationRuleResponseDto>> GetRuleByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await ruleService.GetRuleByIdAsync(
            id, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<Guid>> CreateRuleAsync(
        [FromBody] AutomationRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await ruleService.CreateRuleAsync(
            request, cancellationToken);
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
        var result = await ruleService.UpdateRuleAsync(
            id, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> DeleteRuleAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await ruleService.DeleteRuleAsync(
            id, cancellationToken);

        return this.ToActionResult(result);
    }
}