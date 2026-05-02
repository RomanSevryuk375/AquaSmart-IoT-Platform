using Contracts.Events.EcosystemEvents;
using Contracts.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.BackgroundJobs;
using Telemetry.Infrastructure.Messaging;
using Telemetry.Infrastructure.Messaging.EcosystemConsumers;
using Telemetry.Infrastructure.Messaging.SensorConsumers;
using Telemetry.Infrastructure.Repositories;
using EcosystemCreatedConsumer = Telemetry.Infrastructure.Messaging.EcosystemConsumers.EcosystemCreatedConsumer;

namespace Telemetry.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ITelemetryRawDataRepository, TelemetryRawDataRepository>();
        services.AddScoped<ITelemetryAggregateDataRepository, TelemetryAggregateDataRepository>();

        var connectionString = configuration.GetConnectionString(nameof(SystemDbContext));
        services.AddDbContext<SystemDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddQuartzJob(this IServiceCollection services)
    {
        services.AddQuartz(options =>
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

        services.AddQuartzHostedService(hostOptions 
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var rabbitOptions = rabbitSection.Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.Configure<RabbitMqOptions>(rabbitSection);

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddConsumer<EcosystemCreatedConsumer>();
            busConfigurator.AddConsumer<EcosystemDeletedConsumer>();

            busConfigurator.AddConsumer<SensorCreatedConsumer>();
            busConfigurator.AddConsumer<SensorUpdatedConsumer>();
            busConfigurator.AddConsumer<SensorDeletedConsumer>();
            busConfigurator.AddConsumer<SensorRenamedConsumer>();
            busConfigurator.AddConsumer<SensorStateChangedConsumer>();

            busConfigurator.AddConsumer<TelemetryBatchConsumer>();

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(rabbitOptions.Host), h =>
                {
                    h.Username(rabbitOptions.UserName);
                    h.Password(rabbitOptions.Password);
                });

                configurator.ConfigureEndpoints(context);
            });
        });

        services.AddHealthChecks()
            .AddRabbitMQ(new Uri(rabbitOptions.Host));

        return services;
    }
}
