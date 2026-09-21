// Ignore Spelling: Mq

using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.IntegrationEvents;
using Control.Domain.Interfaces;
using Control.Infrastructure.BackgroundJobs;
using Control.Infrastructure.GrpcClients;
using Control.Infrastructure.Messaging.Relay;
using Control.Infrastructure.Messaging.Sensor;
using Control.Infrastructure.Messaging.Telemetry;
using Control.Infrastructure.Persistence;
using Control.Infrastructure.Persistence.Repositories;
using Control.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Control.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresDbContext<ControlDbContext>(configuration)
                       .AddDapper<ControlDbContext>()
                       .AddRepositories()
                       .AddRabbitMq(configuration)
                       .AddOutboxProcessorQuartzJob<ControlDbContext>(configuration)
                       .AddQuartzJobs()
                       .AddUserContext()
                       .AddCache(configuration);
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IVacationModeRepository, VacationModeRepository>();

        services.AddSingleton<ICronValidator, CronValidator>();
        services.AddScoped<IHardwareValidator, HardwareValidator>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddGlobalMessaging(configuration, cfg =>
        {
            cfg.AddConsumer<RelayCreatedEventConsumer>();
            cfg.AddConsumer<RelayDeletedEventConsumer>();
            cfg.AddConsumer<RelayModeChangedComandConsumer>();
            cfg.AddConsumer<RelayStateChangedComandConsumer>();
            cfg.AddConsumer<RelayUpdatedEventConsumer>();

            cfg.AddConsumer<SensorCreatedEventconsumer>();
            cfg.AddConsumer<SensorDeletedEventConsume>();
            cfg.AddConsumer<SensorNoDataEventConsumer>();
            cfg.AddConsumer<SensorStateChangedComandConsumer>();
            cfg.AddConsumer<SensorUpdatedEventConsumer>();

            cfg.AddConsumer<TelemetryReceivedEventConsumer>();
        });
    }

    private static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartzWithHostedService(opts =>
        {
            var jobKey = new JobKey(nameof(ScheduleProcessJob));
            opts.AddJob<ScheduleProcessJob>(jobOpts => jobOpts.WithIdentity(jobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey}-trigger")
                .WithCronSchedule("0 * * * * ?"));
        });

        return services;
    }
}
