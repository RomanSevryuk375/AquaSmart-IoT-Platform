using Contracts.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Telemetry.Application.Interfaces;
using Telemetry.Application.Services;

namespace Telemetry.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompressorService, CompressorService>();
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IOutboxMessageProcessorService, OutboxMessageProcessorService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ISensorStateCheckerService, SensorStateCheckerService>();
        services.AddScoped<ITelemetryDataService, TelemetryDataService>();
        services.AddScoped<ITelemetryRetentionService, TelemetryRetentionService>();
        services.AddScoped<IDataAggregateService, DataAggregateService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(config =>
            config.AddMaps(typeof(DependencyInjection).Assembly));

        services.Configure<TelemetrySettings>(
            configuration.GetSection(TelemetrySettings.SectionName));

        return services;
    }
}
