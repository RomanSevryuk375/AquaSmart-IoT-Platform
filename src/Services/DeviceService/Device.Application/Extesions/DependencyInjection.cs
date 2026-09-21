using System.Reflection;
using BuildingBlocks.Application.Extensions;
using Device.Application.Behaviors;
using Device.Application.Interfaces;
using Device.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Device.Application.Extesions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IControllerOfflineCheckerService, ControllerOfflineCheckerService>();
        services.AddScoped<IDeviceSecurityService, DeviceSecurityService>();
        services.AddSingleton<IDeviceTokenHasher, HmacDeviceTokenHasher>();
        services.AddSingleton<IMyHasher, MyHasher>();

        services.AddGlobalBehaviors();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ControllerSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CommandSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DeviceSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RelaySecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(SensorSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TelemetrySecurityBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.Configure<DeviceSettings>(configuration.GetSection(DeviceSettings.SectionName));

        return services;
    }
}
