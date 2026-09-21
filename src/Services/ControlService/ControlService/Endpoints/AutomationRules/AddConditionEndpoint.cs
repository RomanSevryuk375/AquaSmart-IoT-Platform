using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.DTOs.AutomationRules;
using Control.Application.Features.AutomationRules.Commands.AddCondition;

namespace Control.API.Endpoints.AutomationRules;

public sealed class AddConditionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.AutomationRules}/{{ruleId:guid}}/conditions", async (
            Guid ruleId,
            RuleConditionRequestDto request,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            AddConditionCommand command = new AddConditionCommand
            {
                UserId = userContext.UserId,
                RuleId = ruleId,
                SensorId = request.SensorId,
                Condition = request.Condition,
                Threshold = request.Threshold,
                Hysteresis = request.Hysteresis,
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Automation Rules Conditions")
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .RequireAuthorization(SubPermissions.AutoRuleCreate);
    }
}