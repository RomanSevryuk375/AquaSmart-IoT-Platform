using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.RelayCommands.Query.GetPending;
using MediatR;

namespace Device.API.Endpoints.RelayCommand;

public sealed class GetPendingCommandsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiConstants.Routes.Commands}/pending/{{controllerId:guid}}", async (
            Guid controllerId,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetPendingCommandsQuery
            {
                ControllerId = controllerId,
                DeviceToken = deviceToken
            };

            Result<IReadOnlyList<RelayCommandDto>> result = await sender.Send(query, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Relay Commands")
        .Produces<IReadOnlyList<RelayCommandDto>>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
