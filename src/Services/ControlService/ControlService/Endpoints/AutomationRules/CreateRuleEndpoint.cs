using AutoMapper;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.DTOs.AutomationRules;
using Control.Application.Features.AutomationRules.Commands.CreateRule;

namespace Control.API.Endpoints.AutomationRules;

public sealed class CreateRuleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiConstants.Routes.AutomationRules, async (
            CreateRuleRequestDto request,
            ISender sender,
            IMapper mapper,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            CreateRuleCommand command = mapper.Map<CreateRuleCommand>(request) with 
            { 
                UserId = userContext.UserId 
            };

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.CreatedAtRoute("GetRuleById", new { id = result.Value }, result.Value)
                : result.ToIResult();
        })
        .WithTags("Automation Rules")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .RequireAuthorization(SubPermissions.AutoRuleCreate);
    }
}