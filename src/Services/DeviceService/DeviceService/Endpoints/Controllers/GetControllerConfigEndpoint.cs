using BuildingBlocks.Domain.Results;
using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using BuildingBlocks.Presentation.ResultExtensions;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using MediatR;

namespace Device.API.Endpoints.Controllers;

public sealed class GetControllerConfigEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet($"{ApiConstants.Routes.Controllers}/me/config", async (
            [FromHeader(Name = ApiConstants.Headers.MacAddress)] string macAddress,
            [FromHeader(Name = ApiConstants.Headers.DeviceToken)] string deviceToken,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetControllerConfigQuery
            {
                MacAddress = macAddress,
                DeviceToken = deviceToken
            };

            Result<ControllerConfig> result = await sender.Send(query, cancellationToken);

            return result.ToIResult();
        })
        .WithTags("Controllers")
        .Produces<ControllerConfig>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous();
    }
}
