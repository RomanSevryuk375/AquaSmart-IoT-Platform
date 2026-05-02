using Contracts.Options;
using Device.Domain.Interfaces;
using Device.Infrastructure.BackgroundJobs;
using Device.Infrastructure.Messaging;
using Device.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Device.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IControllerRepository, ControllerRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<IRelayCommandsQueueRepository, RelayCommandsQueueRepository>();

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

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(options =>
        {
            var offlineControllerJobKey = new JobKey(nameof(CheckOfflineControllersJob));
            var deleteCompletedTaskAsync = new JobKey(nameof(DeleteCompletedCommandsJob));

            options.AddJob<CheckOfflineControllersJob>(jobOptions =>
                jobOptions.WithIdentity(offlineControllerJobKey));

            options.AddJob<DeleteCompletedCommandsJob>(jobOptions =>
                jobOptions.WithIdentity(deleteCompletedTaskAsync));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(offlineControllerJobKey)
                .WithIdentity($"{offlineControllerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(deleteCompletedTaskAsync)
                .WithIdentity($"{deleteCompletedTaskAsync}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(3).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions     
            => hostOptions.WaitForJobsToComplete = true);

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

            busConfigurator.AddConsumer<SensorNoDataConsumer>();
            busConfigurator.AddConsumer<RelayChangeStateConsumer>();

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
}
