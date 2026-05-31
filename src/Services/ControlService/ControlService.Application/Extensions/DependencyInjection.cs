using Control.Application.Interfaces;
using Control.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Control.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddScoped<IAutomationRuleService, AutomationRuleService>();
        services.AddScoped<IRelayService, RelayService>();
        services.AddScoped<IRuleConditionService, RuleConditionService>();
        services.AddScoped<IScheduleProcessor, ScheduleProcessor>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ITelemetryService, TelemetryService>();

        services.AddMediatR(cfg => 
        { 
            cfg.RegisterServicesFromAssembly(assembly);  
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(config => 
        config.AddMaps(assembly));

        return services;
    }
}