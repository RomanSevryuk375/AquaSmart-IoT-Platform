using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.DTOs.AutomationRules;
using Control.Application.Features.AutomationRules.Commands.UpdateRule;

namespace Control.API.Endpoints.AutomationRules;

public sealed class UpdateRuleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.AutomationRules}/{{id:guid}}", async (
            Guid id,
            AutomationRuleUpdateRequestDto request,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            UpdateRuleCommand command = new UpdateRuleCommand
            {
                UserId = userContext.UserId,
                RuleId = id,
                Name = request.Name,
                RelayId = request.RelayId,
                Operator = request.Operator,
                Action = request.Action,
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Automation Rules")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AutoRuleCreate);
    }
}