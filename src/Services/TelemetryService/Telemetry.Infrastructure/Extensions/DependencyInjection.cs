// Ignore Spelling: Mq

using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.IntegrationEvents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.BackgroundJobs;
using Telemetry.Infrastructure.GrpcClients;
using Telemetry.Infrastructure.Messaging.EcosystemConsumers;
using Telemetry.Infrastructure.Messaging.SensorConsumers;
using Telemetry.Infrastructure.Persistence;
using Telemetry.Infrastructure.Persistence.Repositories;
using Telemetry.Infrastructure.SignalR;
using EcosystemCreatedConsumer = Telemetry.Infrastructure.Messaging.EcosystemConsumers.EcosystemCreatedConsumer;

namespace Telemetry.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresDbContext<TelemetryDbContext>(configuration)
                       .AddDapper<TelemetryDbContext>()
                       .AddRepositories()
                       .AddRabbitMq(configuration)
                       .AddOutboxProcessorQuartzJob<TelemetryDbContext>(configuration)
                       .AddQuartzJob()
                       .AddMySignalR()
                       .AddUserContext()
                       .AddCache(configuration);
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ITelemetryRawDataRepository, TelemetryRawDataRepository>();
        services.AddScoped<ITelemetryAggregateDataRepository, TelemetryAggregateDataRepository>();

        services.AddMemoryCache();
        services.AddScoped<DeviceTokenValidator>();
        services.AddScoped<IDeviceTokenValidator>(sp => new CachedDeviceTokenValidator(
            sp.GetRequiredService<DeviceTokenValidator>(),
            sp.GetRequiredService<IMemoryCache>()));

        return services;
    }

    public static IServiceCollection AddQuartzJob(this IServiceCollection services)
    {
        services.AddQuartzWithHostedService(options =>
        {
            var sensorCheckKey = new JobKey(nameof(CheckSensorStateJob));
            options.AddJob<CheckSensorStateJob>(opts => opts.WithIdentity(sensorCheckKey));
            options.AddTrigger(opts => opts
                .ForJob(sensorCheckKey)
                .WithIdentity("CheckSensorState-trigger")
                .WithCronSchedule("0 */2 * * * ?"));

            var minCompressKey = new JobKey(nameof(CompressRawDataToMinutesJob));
            options.AddJob<CompressRawDataToMinutesJob>(opts => opts.WithIdentity(minCompressKey));
            options.AddTrigger(opts => opts
                .ForJob(minCompressKey)
                .WithIdentity("MinuteCompress-trigger")
                .WithCronSchedule("5 * * * * ?"));

            var hourCompressKey = new JobKey(nameof(CompressRawDataToHoursJob));
            options.AddJob<CompressRawDataToHoursJob>(opts => opts.WithIdentity(hourCompressKey));
            options.AddTrigger(opts => opts
                .ForJob(hourCompressKey)
                .WithIdentity("HourCompress-trigger")
                .WithCronSchedule("0 1 * * * ?"));

            var dayCompressKey = new JobKey(nameof(CompressRawDataToDaysJob));
            options.AddJob<CompressRawDataToDaysJob>(opts => opts.WithIdentity(dayCompressKey));
            options.AddTrigger(opts => opts
                .ForJob(dayCompressKey)
                .WithIdentity("DayCompress-trigger")
                .WithCronSchedule("0 5 0 * * ?"));

            var cleanupKey = new JobKey(nameof(CleanUpOldDataJob));
            options.AddJob<CleanUpOldDataJob>(opts => opts.WithIdentity(cleanupKey));
            options.AddTrigger(opts => opts
                .ForJob(cleanupKey)
                .WithIdentity("Cleanup-trigger")
                .WithCronSchedule("0 0 3 * * ?"));
        });

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddGlobalMessaging(configuration, cfg =>
        {
            cfg.AddConsumer<EcosystemCreatedConsumer>();
            cfg.AddConsumer<EcosystemDeletedConsumer>();

            cfg.AddConsumer<SensorCreatedConsumer>();
            cfg.AddConsumer<SensorUpdatedConsumer>();
            cfg.AddConsumer<SensorDeletedConsumer>();
            cfg.AddConsumer<SensorRenamedConsumer>();
            cfg.AddConsumer<SensorStateChangedConsumer>();
        });
    }

    private static IServiceCollection AddMySignalR(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddScoped<ITelemetryNotifier, RawTelemetryNotifier>();

        return services;
    }
}
