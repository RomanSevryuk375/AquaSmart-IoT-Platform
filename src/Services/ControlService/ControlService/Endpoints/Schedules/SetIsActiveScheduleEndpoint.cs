using BuildingBlocks.Domain.Abstractions;

using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.Schedules.Commands.SetIsActiveSchedule;

namespace Control.API.Endpoints.Schedules;

public sealed record SetIsActiveScheduleRequest
{
    public required bool IsActive { get; init; }
}

public sealed class SetIsActiveScheduleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.Schedules}/{{id:guid}}/active", async (
            Guid id,
            SetIsActiveScheduleRequest request,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            SetIsActiveScheduleCommand command = new()
            {
                ScheduleId = id,
                IsActive = request.IsActive,
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