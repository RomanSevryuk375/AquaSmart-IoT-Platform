// Ignore Spelling: Mq

using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.IntegrationEvents;
using Contracts.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.BackgroundJob;
using Notification.Infrastructure.GrpcClients;
using Notification.Infrastructure.Messaging.Alert;
using Notification.Infrastructure.Messaging.Ecosystem;
using Notification.Infrastructure.Messaging.User;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using Notification.Infrastructure.Providers;
using Quartz;

namespace Notification.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresDbContext<NotificationDbContext>(configuration)
                .AddDapper<NotificationDbContext>()
                .AddRepositories()
                .AddRabbitMq(configuration)
                .AddQuartzJobs()
                .AddMessageProviders(configuration)
                .AddUserContext()
                .AddCache(configuration);
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDeviceMetadataEnricher, DeviceMetadataEnricher>();
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddMessageProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        services.AddHttpClient<ITgProvider, TgProvider>();
        services.AddSingleton<IEmailProvider, EmailProvider>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddGlobalMessaging(configuration, cfg =>
        {
            cfg.AddConsumer<EcosystemCreatedEventConsumer>();
            cfg.AddConsumer<EcosystemDeletedEventConsumer>();
            cfg.AddConsumer<EcosystemUpdatedEventConsumer>();

            cfg.AddConsumer<UserCreatedEventConsumer>();
            cfg.AddConsumer<UserUpdatedEventConsumer>();
            cfg.AddConsumer<SubscriptionDowngradedEventConsumer>();

            cfg.AddConsumer<CriticalTelemetryThresholdAlertEventConsumer>();

            cfg.AddConsumer<SensorNoDataAlertEventConsumer>();

            cfg.AddConsumer<ControllerNotOnlineEventConsumer>();
        });
    }

    private static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartzWithHostedService(opts =>
        {
            var reminderJobKey = new JobKey(nameof(ReminderCheckerJob));
            opts.AddJob<ReminderCheckerJob>(jobOpts =>
                jobOpts.WithIdentity(reminderJobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(reminderJobKey)
                .WithIdentity($"{reminderJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()));

            var unpublishedNoticeJobKey = new JobKey(nameof(UnpublishedNoticeProcessorJob));
            opts.AddJob<UnpublishedNoticeProcessorJob>(jobOptions =>
                jobOptions.WithIdentity(unpublishedNoticeJobKey));
            opts.AddTrigger(triggerOptions => triggerOptions
                .ForJob(unpublishedNoticeJobKey)
                .WithIdentity($"{unpublishedNoticeJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));
        });

        return services;
    }
}
