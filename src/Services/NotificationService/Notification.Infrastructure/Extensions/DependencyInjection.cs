// Ignore Spelling: Mq

using Contracts.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Application.Interfaces;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.BackgroundJob;
using Notification.Infrastructure.Factories;
using Notification.Infrastructure.GrpcClients;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using Notification.Infrastructure.Providers;
using Quartz;

namespace Notification.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddRepositories(configuration)
                .AddMessageProviders(configuration)
                .AddRabbitMq(configuration)
                .AddQuartzJobs();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDeviceMetadataEnricher, DeviceMetadataEnricher>();

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        string? connectionSting = configuration.GetConnectionString(nameof(NotificationDbContext));
        services.AddDbContext<NotificationDbContext>(options =>
        {
            options.UseNpgsql(connectionSting)
                   .UseSnakeCaseNamingConvention();
        });
        services.AddHealthChecks().AddNpgSql(connectionSting!);

        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddHttpContextAccessor();

        services.AddHostedService<DatabaseMigrationService>();

        return services;
    }

    public static IServiceCollection AddMessageProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        services.AddHttpClient<ITgProvider, TgProvider>();
        services.AddSingleton<IEmailProvider, EmailProvider>();

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        RabbitMqOptions rabbitOgtions = rabbitSection.Get<RabbitMqOptions>()
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
            busConfigurator.AddConsumer<SubscriptionDowngradedEventConsumer>();

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
        services.AddQuartz(opts =>
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

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }
}

internal sealed class DatabaseMigrationService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        NotificationDbContext context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
