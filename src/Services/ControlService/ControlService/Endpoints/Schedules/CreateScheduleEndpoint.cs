using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.Schedules.Commands.CreateSchedule;

namespace Control.API.Endpoints.Schedules;

public sealed class CreateScheduleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiConstants.Routes.Schedules, async (
            CreateScheduleCommand command,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            CreateScheduleCommand enrichedCommand = command with { UserId = userContext.UserId };

            Result<Guid> result = await sender.Send(enrichedCommand, cancellationToken);

            return result.IsSuccess
                ? Results.CreatedAtRoute("GetScheduleById", new { id = result.Value }, result.Value)
                : result.ToIResult();
        })
        .WithTags("Schedules")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AutoScheduleCreate);
    }
}