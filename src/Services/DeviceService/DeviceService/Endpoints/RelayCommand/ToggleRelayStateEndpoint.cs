using BuildingBlocks.Domain.Abstractions;

using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.RelayCommands.Command.ToggleRelayState;
using MediatR;

namespace Device.API.Endpoints.RelayCommand;

public sealed class ToggleRelayStateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Commands}/toggle-state/{{relayId:guid}}", async (
            Guid relayId,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            var command = new ToggleRelayStateCommand
            {
                RelayId = relayId,
                UserId = userContext.UserId
            };

            Result<bool> result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Relay Commands")
        .Produces<bool>()
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.DeviceControl);
    }
}
