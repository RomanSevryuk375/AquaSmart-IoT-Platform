using Contracts.Constants;

namespace Notification.API.Extensions;

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

        return application;
    }
}
