using Device.API.gRPC;

namespace Device.API.Extensions;

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
        application.MapGrpcService<DeviceIntegrationEndpoint>();

        return application;
    }
}
