using Contracts.Options;
using Control.Domain.Interfaces;
using Control.Infrastructure.BackgroundJobs;
using Control.Infrastructure.Messaging.Relay;
using Control.Infrastructure.Messaging.Sensor;
using Control.Infrastructure.Messaging.Telemetry;
using Control.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Control.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEcosystemRepository, EcosystemRepository>();
        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IVacationModeRepository, VacationModeRepository>();

        var connectionString = configuration.GetConnectionString(nameof(SystemDbContext));

        services.AddDbContext<SystemDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
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
        services.AddQuartz(options =>
        {
            var jobKey = new JobKey(nameof(ScheduleProcessJob));

            options.AddJob<ScheduleProcessJob>(jobOptions =>
                jobOptions.WithIdentity(jobKey));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(jobKey)
                .WithIdentity($"{jobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }
}
