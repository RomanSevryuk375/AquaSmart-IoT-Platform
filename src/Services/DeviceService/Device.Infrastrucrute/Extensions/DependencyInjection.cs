using Contracts.Options;
using Device.Application.Interfaces;
using Device.Infrastructure.BackgroundJobs;
using Device.Infrastructure.Messaging;
using Device.Infrastructure.Persistence.Interceptors;
using Device.Infrastructure.Persistence.Outbox;
using Device.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Device.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddScoped<IControllerRepository, ControllerRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IRelayCommandsQueueRepository, RelayCommandsQueueRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        var connectionString = configuration.GetConnectionString(nameof(SystemDbContext));

        services.AddDbContext<SystemDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

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

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(options =>
        {
            
            var deleteCompletedTaskAsync = new JobKey(nameof(DeleteCompletedCommandsJob));
            options.AddJob<DeleteCompletedCommandsJob>(jobOptions =>
                jobOptions.WithIdentity(deleteCompletedTaskAsync));
            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(deleteCompletedTaskAsync)
                .WithIdentity($"{deleteCompletedTaskAsync}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(3).RepeatForever()));

            var offlineControllerJobKey = new JobKey(nameof(CheckOfflineControllersJob));
            options.AddJob<CheckOfflineControllersJob>(jobOptions =>
                jobOptions.WithIdentity(offlineControllerJobKey));
            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(offlineControllerJobKey)
                .WithIdentity($"{offlineControllerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever()));

            var outboxKey = new JobKey(nameof(OutboxMessageProcessorJob));
            options.AddJob<OutboxMessageProcessorJob>(jobOptions =>
                jobOptions.WithIdentity(outboxKey));
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
