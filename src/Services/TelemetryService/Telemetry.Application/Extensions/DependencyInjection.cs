using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Interfaces;
using Telemetry.Application.Services;

namespace Telemetry.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICompressorService, CompressorService>();
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IOutboxMessageProcessorService, OutboxMessageProcessorService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ISensorStateCheckerService, SensorStateCheckerService>();
        services.AddScoped<ITelemetryDataService, TelemetryDataService>();
        services.AddScoped<ITelemetryRetentionService, TelemetryRetentionService>();
        services.AddScoped<IDataAggregateService, DataAggregateService>();

        return services;
    }
}
