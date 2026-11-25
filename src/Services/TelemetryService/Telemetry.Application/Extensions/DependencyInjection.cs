using System.Reflection;
using BuildingBlocks.Application.Extensions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Features.BackgroundJobs.Commands.Shared;
using Telemetry.Application.Interfaces;
using Telemetry.Application.Services;

namespace Telemetry.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompressorHelper, CompressorHelper>();
        services.AddScoped<ITelemetryBatchIngestionService, TelemetryBatchIngestionService>();

        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.AddGlobalBehaviors();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.Configure<TelemetrySettings>(configuration.GetSection(TelemetrySettings.SectionName));

        return services;
    }
}
