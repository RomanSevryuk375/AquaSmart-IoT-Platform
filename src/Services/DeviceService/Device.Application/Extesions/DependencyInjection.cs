using System.Reflection;
using Device.Application.Behaviors;
using Device.Application.Interfaces;
using Device.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Device.Application.Extesions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IControllerOfflineCheckerService, ControllerOfflineCheckerService>();
        services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
        services.AddScoped<IDeviceSecurityService, DeviceSecurityService>();

        services.AddSingleton<IMyHasher, MyHasher>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ControllerSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CommandSecurityBehaviod<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DeviceSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RelaySecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(SensorSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TelemetrySecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(config => config.AddMaps(assembly));

        services.Configure<DeviceSettings>(configuration.GetSection(DeviceSettings.SectionName));

        return services;
    }
}
