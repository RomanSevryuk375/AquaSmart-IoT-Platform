using Contracts.Options;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISubscriptionExpiredChecker, SubscriptionExpiredChecker>();
        services.AddScoped<IIncorrectTokenChecker, IncorrectTokenChecker>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IMyHasher, MyHasher>();

        return services;
    }
}
