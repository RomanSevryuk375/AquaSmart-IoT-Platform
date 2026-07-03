using Contracts.Authorization;
using Contracts.Results;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.RuleConditions.Command.AddCondition;
using Control.Application.Features.RuleConditions.Command.DeleteCondition;
using Control.Application.Features.RuleConditions.Command.UpdateCondition;
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
        AddConditionCommand command = new AddConditionCommand
        {
            RuleId = ruleId,
            SensorId = request.SensorId,
            Condition = request.Condition,
            Threshold = request.Threshold,
            Hysteresis = request.Hysteresis,
        };
        Result<Guid> result = await sender.Send(command, cancellationToken);

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
        UpdateConditionCommand command = new UpdateConditionCommand
        {
            RuleId = ruleId,
            ConditionId = conditionId,
            SensorId = request.SensorId,
            Condition = request.Condition,
            Threshold = request.Threshold,
            Hysteresis = request.Hysteresis,
        };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{conditionId:guid}")]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<IActionResult> DeleteConditionAsync(
        [FromRoute] Guid ruleId,
        [FromRoute] Guid conditionId,
        CancellationToken cancellationToken)
    {
        DeleteConditionCommand command = new DeleteConditionCommand
        {
            ConditionId = conditionId,
            RuleId = ruleId,
        };
        Result result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }
}