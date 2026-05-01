using Control.Application.Interfaces;
using Control.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Control.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAutomationRuleService, AutomationRuleService>();
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IRelayService, RelayService>();
        services.AddScoped<IRuleConditionService, RuleConditionService>();
        services.AddScoped<IScheduleProcessor, ScheduleProcessor>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ITelemetryService, TelemetryService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddAutoMapper(config =>
            config.AddMaps(typeof(DependencyInjection).Assembly));

        return services;
    }
}