using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Behaviors;
using Telemetry.Application.Features.BackgroundJobs.Commands.Shared;
using Telemetry.Application.Interfaces;

namespace Telemetry.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompressorHelper, CompressorHelper>();

        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.Configure<TelemetrySettings>(
            configuration.GetSection(TelemetrySettings.SectionName));

        return services;
    }
}
