using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.Features.Ecosystems.Commands.DeleteEcosystem;

namespace Control.API.Endpoints.Ecosystems;

public sealed class DeleteEcosystemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete($"{ApiConstants.Routes.Ecosystems}/{{id:guid}}", async (
            Guid id,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            DeleteEcosystemCommand command = new()
            {
                EcosystemId = id,
                UserId = userContext.UserId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Ecosystems")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.TankDelete);
    }
}