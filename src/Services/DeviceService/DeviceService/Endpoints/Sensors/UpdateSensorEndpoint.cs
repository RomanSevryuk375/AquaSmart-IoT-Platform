using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Authorization;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.Sensors.Command.UpdateSensor;
using MediatR;

namespace Device.API.Endpoints.Sensors;

public sealed class UpdateSensorEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut($"{ApiConstants.Routes.Sensors}/{{id:guid}}", async (
            Guid id,
            UpdateSensorCommand command,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            UpdateSensorCommand enrichedCommand = command with { SensorId = id };

            Result result = await sender.Send(enrichedCommand, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Sensors")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(SubPermissions.DeviceControl);
    }
}
