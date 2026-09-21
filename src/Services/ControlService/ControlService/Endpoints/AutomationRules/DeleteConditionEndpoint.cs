using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.AutomationRules.Commands.DeleteCondition;

namespace Control.API.Endpoints.AutomationRules;

public sealed class DeleteConditionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{ApiConstants.Routes.AutomationRules}/{{ruleId:guid}}/conditions/{{conditionId:guid}}", async (
            Guid ruleId,
            Guid conditionId,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            DeleteConditionCommand command = new DeleteConditionCommand
            {
                UserId = userContext.UserId,
                RuleId = ruleId,
                ConditionId = conditionId,
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Automation Rules Conditions")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AutoRuleCreate);
    }
}