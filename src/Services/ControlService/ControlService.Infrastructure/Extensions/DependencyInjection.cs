using Contracts.Options;
using Control.Domain.Interfaces;
using Control.Infrastructure.BackgroundJobs;
using Control.Infrastructure.Messaging.Relay;
using Control.Infrastructure.Messaging.Sensor;
using Control.Infrastructure.Messaging.Telemetry;
using Control.Infrastructure.Persistence;
using Control.Infrastructure.Persistence.Interceptors;
using Control.Infrastructure.Persistence.Outbox;
using Control.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Control.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IRuleConditionRepository, RuleConditionRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IVacationModeRepository, VacationModeRepository>();

        string? connectionString = configuration.GetConnectionString(nameof(SystemDbContext));

        services.AddDbContext<SystemDbContext>((sp, options) =>
        {
            ConvertDomainEventsToOutboxMessagesInterceptor interceptor = sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

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
            busConfigurator.AddDelayedMessageScheduler();
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddConsumer<RelayCreatedEventConsumer>();
            busConfigurator.AddConsumer<RelayDeletedEventConsumer>();
            busConfigurator.AddConsumer<RelayModeChangedComandConsumer>();
            busConfigurator.AddConsumer<RelayStateChangedComandConsumer>();
            busConfigurator.AddConsumer<RelayUpdatedEventConsumer>();

            busConfigurator.AddConsumer<SensorCreatedEventconsumer>();
            busConfigurator.AddConsumer<SensorDeletedEventConsume>();
            busConfigurator.AddConsumer<SensorNoDataEventConsumer>();
            busConfigurator.AddConsumer<SensorStateChangedComandConsumer>();
            busConfigurator.AddConsumer<SensorUpdatedEventConsumer>();

            busConfigurator.AddConsumer<TelemetryReceivedEventConsumer>();

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseDelayedMessageScheduler();

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
            var jobKey = new JobKey(nameof(ScheduleProcessJob));
            opts.AddJob<ScheduleProcessJob>(jobOpts => jobOpts.WithIdentity(jobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));

            var outboxJobKey = new JobKey(nameof(OutboxMessageProcessorJob));
            opts.AddJob<OutboxMessageProcessorJob>(jobOpts => jobOpts.WithIdentity(outboxJobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(outboxJobKey)
                .WithIdentity($"{outboxJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }
}
