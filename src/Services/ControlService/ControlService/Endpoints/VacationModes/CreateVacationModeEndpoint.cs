using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.VacationModes.Commands.CreateVacationMode;

namespace Control.API.Endpoints.VacationModes;

public sealed class CreateVacationModeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiConstants.Routes.VacationModes, async (
            CreateVacationModeCommand command,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            CreateVacationModeCommand enrichedCommand = command with { UserId = userContext.UserId };

            Result<Guid> result = await sender.Send(enrichedCommand, cancellationToken);

            return result.IsSuccess
                ? Results.CreatedAtRoute("GetVacationModeById", new { id = result.Value }, result.Value)
                : result.ToIResult();
        })
        .WithTags("Vacation Modes")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.VacationMode);
    }
}