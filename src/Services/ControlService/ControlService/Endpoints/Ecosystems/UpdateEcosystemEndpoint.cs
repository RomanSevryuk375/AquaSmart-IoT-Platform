using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Control.Application.DTOs.Ecosystems;
using Control.Application.Features.Ecosystems.Commands.UpdateEcosystem;

namespace Control.API.Endpoints.Ecosystems;

public sealed class UpdateEcosystemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.Ecosystems}/{{id:guid}}", async (
            Guid id,
            EcosystemUpdateRequestDto request,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            UpdateEcosystemCommand command = new UpdateEcosystemCommand
            {
                UserId = userContext.UserId,
                EcosystemId = id,
                Name = request.Name,
                Volume = request.Volume
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Ecosystems")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.TankUpdate);
    }
}