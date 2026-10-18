using Mobile.Services;

namespace Mobile.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAquariumService, AquariumService>();
        services.AddSingleton<IKnowledgeService, KnowledgeService>();

        return services;
    }
}
