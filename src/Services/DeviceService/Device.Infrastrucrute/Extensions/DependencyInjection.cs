// Ignore Spelling: Mq

using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.IntegrationEvents;
using Device.Infrastructure.BackgroundJobs;
using Device.Infrastructure.Messaging;
using Device.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Device.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresDbContext<DeviceDbContext>(configuration)
                       .AddDapper<DeviceDbContext>()
                       .AddRepositories()
                       .AddRabbitMq(configuration)
                       .AddOutboxProcessorQuartzJob<DeviceDbContext>(configuration)
                       .AddQuartzJobs(configuration)
                       .AddUserContext()
                       .AddCache(configuration);
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IControllerRepository, ControllerRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IRelayCommandsRepository, RelayCommandsQueueRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        return services;
    }

    private static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration)
    {
        BackgroundJobsOptions backgroundJobOptions = configuration
            .GetSection(BackgroundJobsOptions.SectionName)
            .Get<BackgroundJobsOptions>() ?? throw new InvalidOperationException(DiErrors.BackgroundJobsConfiguration);

        services.AddQuartzWithHostedService(opts =>
        {
            var deleteCompletedTaskAsync = new JobKey(nameof(DeleteCompletedCommandsJob));
            opts.AddJob<DeleteCompletedCommandsJob>(jobOpts =>
                jobOpts.WithIdentity(deleteCompletedTaskAsync));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(deleteCompletedTaskAsync)
                .WithIdentity($"{deleteCompletedTaskAsync}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(backgroundJobOptions.DeleteCompletedCommandsIntervalHours)
                .RepeatForever()));

            var offlineControllerJobKey = new JobKey(nameof(CheckOfflineControllersJob));
            opts.AddJob<CheckOfflineControllersJob>(jobOpts =>
                jobOpts.WithIdentity(offlineControllerJobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(offlineControllerJobKey)
                .WithIdentity($"{offlineControllerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(backgroundJobOptions.OfflineCheckerIntervalSeconds)
                .RepeatForever()));
        });

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddGlobalMessaging(configuration, cfg =>
        {
            cfg.AddConsumer<SensorNoDataConsumer>();
            cfg.AddConsumer<RelayChangeStateConsumer>();
        });
    }
}
