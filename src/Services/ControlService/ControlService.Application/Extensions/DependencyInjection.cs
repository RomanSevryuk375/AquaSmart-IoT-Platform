using System.Reflection;
using Control.Application.Features.Common.Behaviors;
using Control.Application.Interfaces;
using Control.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Control.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IRelayService, RelayService>();
        services.AddScoped<IScheduleProcessor, ScheduleProcessor>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ITelemetryService, TelemetryService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(EcosystemSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AutomationRuleSecurityBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(config =>
        config.AddMaps(assembly));

        return services;
    }
}
