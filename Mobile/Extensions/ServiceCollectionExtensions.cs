using System.Net.Security;
using Mobile.Services;

namespace Mobile.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<AuthHeaderHandler>();

        services.AddSingleton<HttpClient>(sp =>
        {
            var authHandler = sp.GetRequiredService<AuthHeaderHandler>();
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            };
            authHandler.InnerHandler = socketsHandler;

            return new HttpClient(authHandler)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        });

        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAquariumService, AquariumService>();
        services.AddSingleton<IKnowledgeService, KnowledgeService>();
        services.AddSingleton<IControllerService, ControllerService>();
        services.AddSingleton<ITelemetryHubService, TelemetryHubService>();

        return services;
    }
}
