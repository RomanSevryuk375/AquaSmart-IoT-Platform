using Contracts.Authorization;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/automation-rules/{ruleId:guid}/conditions")]
public class RuleConditionsController(
    IRuleConditionService conditionService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<Guid>> AddConditionAsync(
        [FromRoute] Guid ruleId,
        [FromBody] RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await conditionService.AddConditionAsync(
            ruleId, request, cancellationToken);

        return this.ToActionResult(result);
    }


    [HttpPut("{conditionId:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)] 
    public async Task<IActionResult> UpdateConditionAsync(
        [FromRoute] Guid ruleId,
        [FromRoute] Guid conditionId,
        [FromBody] RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await conditionService.UpdateConditionAsync(
            ruleId, conditionId, request, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{conditionId:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> DeleteConditionAsync(
        [FromRoute] Guid ruleId,
        [FromRoute] Guid conditionId,
        CancellationToken cancellationToken)
    {
        var result = await conditionService.DeleteConditionAsync(
            ruleId, conditionId, cancellationToken);

        return this.ToActionResult(result);
    }
}