using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.VacationModes.Commands.ToggleVacationMode;

namespace Control.API.Endpoints.VacationModes;

public sealed class ToggleVacationModeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.VacationModes}/{{id:guid}}/toggle", async (
            Guid id,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            ToggleVacationModeCommand command = new()
            {
                VacationModeId = id,
                UserId = userContext.UserId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Vacation Modes")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.VacationMode);
    }
}