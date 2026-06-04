using Contracts.Authorization;
using Contracts.Results;
using Control.Application.CQRS.RuleCondition.Command.AddCondition;
using Control.Application.CQRS.RuleCondition.Command.DeleteCondition;
using Control.Application.CQRS.RuleCondition.Command.UpdateCondition;
using Control.Application.DTOs.AutomationRule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/automation-rules/{ruleId:guid}/conditions")]
public class RuleConditionsController(
    ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<Guid>> AddConditionAsync(
        [FromRoute] Guid ruleId,
        [FromBody] RuleConditionRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddConditionCommand
        {
            RuleId = ruleId,
            SensorId = request.SensorId,
            Condition = request.Condition,
            Threshold = request.Threshold,
            Hysteresis = request.Hysteresis,
        };
        var result = await sender.Send(command, cancellationToken);

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
        var command = new UpdateConditionCommand
        {
            RuleId = ruleId,
            ConditionId = conditionId,
            SensorId = request.SensorId,
            Condition = request.Condition,
            Threshold = request.Threshold,
            Hysteresis = request.Hysteresis,
        };
        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{conditionId:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> DeleteConditionAsync(
        [FromRoute] Guid ruleId,
        [FromRoute] Guid conditionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteConditionCommand
        {
            ConditionId = conditionId,
            RuleId = ruleId,
        };
        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}