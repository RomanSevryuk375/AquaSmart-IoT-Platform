using Contracts.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.BackgroundJob;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Providers;
using Notification.Infrastructure.Repositories;
using Quartz;

namespace Notification.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        var connectionSting = configuration.GetConnectionString(nameof(NotificationDbContext));
        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseNpgsql(connectionSting).UseSnakeCaseNamingConvention();
        });
        services.AddHealthChecks().AddNpgSql(connectionSting!);

        services.AddHttpClient<INotificationProvider, TgProvider>();
        services.AddSingleton<INotificationProvider, EmailProvider>();

        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var rabbitOgtions = rabbitSection.Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.Configure<RabbitMqOptions>(rabbitSection);

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddConsumer<EcosystemCreatedEventConsumer>();
            busConfigurator.AddConsumer<EcosystemDeletedEventConsumer>();
            busConfigurator.AddConsumer<EcosystemUpdatedEventConsumer>();

            busConfigurator.AddConsumer<UserCreatedEventConsumer>();
            busConfigurator.AddConsumer<UserUpdatedEventConsumer>();

            busConfigurator.AddConsumer<CriticalTelemetryThresholdAlertEventConsumer>();

            busConfigurator.AddConsumer<SensorNoDataAlertEventConsumer>();

            busConfigurator.AddConsumer<ControllerNotOnlineEventConsumer>();

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(rabbitOgtions.Host), h =>
                {
                    h.Username(rabbitOgtions.UserName);
                    h.Password(rabbitOgtions.Password);
                });

                configurator.ConfigureEndpoints(context);
            });
        });

        services.AddHealthChecks().AddRabbitMQ(new Uri(rabbitOgtions.Host));

        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(options =>
        {
            var reminderJobKey = new JobKey(nameof(ReminderCheckerJob));

            var unpublishedNoticeJobKey = new JobKey(nameof(UnpublishedNoticeProcessorJob));

            options.AddJob<ReminderCheckerJob>(jobOptions =>
                jobOptions.WithIdentity(reminderJobKey));

            options.AddJob<UnpublishedNoticeProcessorJob>(jobOptions =>
                jobOptions.WithIdentity(unpublishedNoticeJobKey));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(reminderJobKey)
                .WithIdentity($"{reminderJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(unpublishedNoticeJobKey)
                .WithIdentity($"{unpublishedNoticeJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }
}
