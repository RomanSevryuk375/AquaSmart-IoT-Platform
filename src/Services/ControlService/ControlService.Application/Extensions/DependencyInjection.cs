using Control.Application.Interfaces;
using Control.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Control.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IAutomationRuleService, AutomationRuleService>();
        services.AddScoped<IRelayService, RelayService>();
        services.AddScoped<IScheduleProcessor, ScheduleProcessor>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ITelemetryServiceFromEvent, TelemetryService>();

        return services;
    }
}