// Ignore Spelling: Mq

using Contracts.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.BackgroundJobs;
using Telemetry.Infrastructure.Factories;
using Telemetry.Infrastructure.Messaging;
using Telemetry.Infrastructure.Messaging.EcosystemConsumers;
using Telemetry.Infrastructure.Messaging.SensorConsumers;
using Telemetry.Infrastructure.Persistence;
using Telemetry.Infrastructure.Persistence.Interceptors;
using Telemetry.Infrastructure.Persistence.Outbox;
using Telemetry.Infrastructure.Persistence.Repositories;
using Telemetry.Infrastructure.SignalR;
using EcosystemCreatedConsumer = Telemetry.Infrastructure.Messaging.EcosystemConsumers.EcosystemCreatedConsumer;

namespace Telemetry.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddRepositories(configuration)
                       .AddQuartzJob()
                       .AddRabbitMq(configuration)
                       .AddMySignalR();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ITelemetryRawDataRepository, TelemetryRawDataRepository>();
        services.AddScoped<ITelemetryAggregateDataRepository, TelemetryAggregateDataRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddScoped<OutboxMessageProcessorService>();

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        string? connectionString = configuration.GetConnectionString(nameof(TelemetryDbContext));
        services.AddDbContext<TelemetryDbContext>((sp, options) =>
        {
            ConvertDomainEventsToOutboxMessagesInterceptor interceptor =
            sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });
        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<DatabaseMigrationService>();

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

            var outboxKey = new JobKey(nameof(OutboxMessageProcessorJob));
            options.AddJob<OutboxMessageProcessorJob>(opts => opts.WithIdentity(outboxKey));
            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(outboxKey)
                .WithIdentity($"{outboxKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        RabbitMqOptions rabbitOptions = rabbitSection.Get<RabbitMqOptions>()
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

        services.AddHealthChecks().AddRabbitMQ(new Uri(rabbitOptions.Host));

        return services;
    }

    public static IServiceCollection AddMySignalR(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddScoped<ITelemetryNotifier, RawTelemetryNotifier>();

        return services;
    }
}

internal sealed class DatabaseMigrationService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        TelemetryDbContext context = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
