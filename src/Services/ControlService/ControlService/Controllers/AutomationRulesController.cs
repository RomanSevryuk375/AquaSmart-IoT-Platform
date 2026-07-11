using AutoMapper;
using Control.Application.DTOs.AutomationRule;
using Control.Application.Features.AutomationRules.Commands.AddCondition;
using Control.Application.Features.AutomationRules.Commands.CreateRule;
using Control.Application.Features.AutomationRules.Commands.DeleteCondition;
using Control.Application.Features.AutomationRules.Commands.DeleteRule;
using Control.Application.Features.AutomationRules.Commands.UpdateCondition;
using Control.Application.Features.AutomationRules.Commands.UpdateRule;
using Control.Application.Features.AutomationRules.Queries;
using Control.Application.Features.AutomationRules.Queries.GetAllRules;
using Control.Application.Features.AutomationRules.Queries.GetRuleById;

namespace Control.API.Controllers;

[ApiController]
[Route(ApiConstants.Routes.AutomationRules)]
public class AutomationRulesController(
    IUserContext userContext,
    ISender sender,
    IMapper mapper) : ControllerBase
{
    private const string GetRuleByIdRoute = "GetRuleById";

    [HttpGet]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<IReadOnlyList<AutomationRuleDto>>> GetAllRulesAsync(
        [FromQuery] GetAllRulesQuery query,
        CancellationToken cancellationToken = default)
    {
        GetAllRulesQuery enrichedQuery = query with { UserId = userContext.UserId };
        Result<IReadOnlyList<AutomationRuleDto>> result = await sender.Send(enrichedQuery, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = GetRuleByIdRoute)]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<AutomationRuleDto>> GetRuleByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetRuleByIdQuery query = new GetRuleByIdQuery { RuleId = id, UserId = userContext.UserId };
        Result<AutomationRuleDto> result = await sender.Send(query, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = SubPermissions.AutoRuleCreate)]
    public async Task<ActionResult<Guid>> CreateRuleAsync(
        [FromBody] CreateRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        CreateRuleCommand command = mapper.Map<CreateRuleCommand>(request);

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


    [HttpPost("{ruleId:guid}/conditions")]
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

    [HttpPut("{ruleId:guid}/conditions/{conditionId:guid}")]
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

    [HttpDelete("{ruleId:guid}/conditions/{conditionId:guid}")]
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