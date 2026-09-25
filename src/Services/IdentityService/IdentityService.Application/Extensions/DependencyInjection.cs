using System.Reflection;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Presentation.Authorization;
using FluentValidation;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddSingleton<IMyHasher, MyHasher>();

        services.AddGlobalBehaviors();

        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        return services;
    }
}
