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
            var innerHandler = new HttpClientHandler();
            innerHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            authHandler.InnerHandler = innerHandler;

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

        return services;
    }
}
