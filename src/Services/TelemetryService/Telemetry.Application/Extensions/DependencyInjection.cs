using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Interfaces;
using Telemetry.Application.Services;

namespace Telemetry.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddScoped<ICompressorService, CompressorService>();
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IOutboxMessageProcessorService, OutboxMessageProcessorService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ISensorStateCheckerService, SensorStateCheckerService>();
        services.AddScoped<ITelemetryDataService, TelemetryDataService>();
        services.AddScoped<ITelemetryRetentionService, TelemetryRetentionService>();
        services.AddScoped<IDataAggregateService, DataAggregateService>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(cfg =>
            cfg.AddMaps(typeof(DependencyInjection).Assembly));

        services.Configure<TelemetrySettings>(
            cfg.GetSection(TelemetrySettings.SectionName));

        return services;
    }
}
