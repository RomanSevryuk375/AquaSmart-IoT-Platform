using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.Controllers.Command.PingController;
using MediatR;

namespace Device.API.Endpoints.Controllers;

public sealed class PingControllerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Controllers}/{{id:guid}}/ping", async (
            Guid id,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new PingControllerCommand
            {
                ControllerId = id,
                DeviceToken = deviceToken
            };

            Result<ControllerPingResponse> result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Controllers")
        .Produces<ControllerPingResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
