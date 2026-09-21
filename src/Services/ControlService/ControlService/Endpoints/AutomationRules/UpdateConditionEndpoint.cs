using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.DTOs.AutomationRules;
using Control.Application.Features.AutomationRules.Commands.UpdateCondition;

namespace Control.API.Endpoints.AutomationRules;

public sealed class UpdateConditionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.AutomationRules}/{{ruleId:guid}}/conditions/{{conditionId:guid}}", async (
            Guid ruleId,
            Guid conditionId,
            RuleConditionRequestDto request,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            UpdateConditionCommand command = new UpdateConditionCommand
            {
                UserId = userContext.UserId,
                RuleId = ruleId,
                ConditionId = conditionId,
                SensorId = request.SensorId,
                Condition = request.Condition,
                Threshold = request.Threshold,
                Hysteresis = request.Hysteresis,
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Automation Rules Conditions")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AutoRuleCreate);
    }
}