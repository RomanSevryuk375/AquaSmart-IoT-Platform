using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.API.Filters;
using Device.Application.Features.RelayCommands.Command.MarkAsFailed;
using MediatR;

namespace Device.API.Endpoints.RelayCommand;

public sealed class MarkCommandAsFailedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost($"{ApiConstants.Routes.Commands}/{{commandId:guid}}/fail", async (
            Guid commandId,
            [FromBody] string errorMessage,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new MarkAsFailedCommand
            {
                CommandId = commandId,
                ErrorMessage = errorMessage
            };

            Result result = await sender.Send(command, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Relay Commands")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AddEndpointFilter<DeviceAuthenticationFilter>();
    }
}
