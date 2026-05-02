using Device.Application.Interfaces;
using Device.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Device.Application.Extesions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices (this IServiceCollection services)
    {
        services.AddScoped<IControllerOfflineCheckerService, ControllerOfflineCheckerService>();
        services.AddScoped<IControllerService, ControllerService>();
        services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
        services.AddScoped<IRelayCommandQueueService, RelayCommandQueueService>();
        services.AddScoped<IRelayService, RelayService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ITelemtryBatchService, TelemetryBatchService>();

        services.AddSingleton<IMyHasher, MyHasher>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddAutoMapper(config => 
            config.AddMaps(typeof(DependencyInjection).Assembly));

        return services;
    }
}
