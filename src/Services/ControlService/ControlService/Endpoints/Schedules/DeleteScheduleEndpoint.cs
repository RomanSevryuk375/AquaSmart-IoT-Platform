using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.Schedules.Commands.DeleteSchedule;

namespace Control.API.Endpoints.Schedules;

public sealed class DeleteScheduleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{ApiConstants.Routes.Schedules}/{{id:guid}}", async (
            Guid id,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            DeleteScheduleCommand command = new()
            {
                ScheduleId = id,
                UserId = userContext.UserId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Schedules")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.AutoScheduleCreate);
    }
}