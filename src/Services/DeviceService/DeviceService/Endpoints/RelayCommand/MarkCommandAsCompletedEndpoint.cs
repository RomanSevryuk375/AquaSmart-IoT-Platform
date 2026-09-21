using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.RelayCommands.Command.MarkAsCompleted;
using MediatR;

namespace Device.API.Endpoints.RelayCommand;

public sealed class MarkCommandAsCompletedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Commands}/{{commandId:guid}}/complete", async (
            Guid commandId,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new MarkAsCompletedCommand
            {
                CommandId = commandId,
                DeviceToken = deviceToken
            };

            Result result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Relay Commands")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
