using Contracts.Constants;
using Telemetry.Infrastructure.SignalR;

namespace Telemetry.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication AddConfiguration(this WebApplication application)
    {
        application.UseSwagger();
        application.UseSwaggerUI();
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapHealthChecks(ApiConstants.HealthRoute);
        application.MapControllers();
        application.MapHub<TelemetryHub>(SignalRRoutes.RawTelemetry);

        return application;
    }
}
