using BuildingBlocks.Presentation.Constants;
using BuildingBlocks.Presentation.Endpoints;
using Telemetry.Infrastructure.SignalR;

namespace Telemetry.API.Endpoints;

public sealed class TelemetryHubEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapHub<TelemetryHub>(SignalRRoutes.RawTelemetry);
}
