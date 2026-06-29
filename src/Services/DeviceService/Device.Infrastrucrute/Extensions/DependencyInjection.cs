// Ignore Spelling: Mq

using Contracts.Options;
using Device.Application.Interfaces;
using Device.Infrastructure.BackgroundJobs;
using Device.Infrastructure.Factories;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRepositories(configuration)
            .AddQuartzJobs(configuration)
            .AddRabbitMq(configuration);
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddScoped<IControllerRepository, ControllerRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IRelayRepository, RelayRepository>();
        services.AddScoped<IRelayCommandsRepository, RelayCommandsQueueRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        string? connectionString = configuration.GetConnectionString(nameof(SystemDbContext));
        services.AddDbContext<SystemDbContext>((sp, options) =>
        {
            ConvertDomainEventsToOutboxMessagesInterceptor interceptor =
            sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });
        services.AddHealthChecks().AddNpgSql(connectionString!);

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserContext, UserContext>();

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<OutboxMessageProcessorService>();

        IConfigurationSection backgroudSection = configuration.GetSection(BackgroundJobsOptions.SectionName);
        BackgroundJobsOptions backgroudJobOptions = backgroudSection.Get<BackgroundJobsOptions>()
            ?? throw new InvalidOperationException(DiErrors.BackgroundJobsConfiguration);
        services.Configure<BackgroundJobsOptions>(backgroudSection);
        services.AddQuartz(opts =>
        {
            var deleteCompletedTaskAsync = new JobKey(nameof(DeleteCompletedCommandsJob));
            opts.AddJob<DeleteCompletedCommandsJob>(jobOpts =>
                jobOpts.WithIdentity(deleteCompletedTaskAsync));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(deleteCompletedTaskAsync)
                .WithIdentity($"{deleteCompletedTaskAsync}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(backgroudJobOptions.DeleteCompletedCommandsIntervalHours)
                .RepeatForever()));

            var offlineControllerJobKey = new JobKey(nameof(CheckOfflineControllersJob));
            opts.AddJob<CheckOfflineControllersJob>(jobOpts =>
                jobOpts.WithIdentity(offlineControllerJobKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(offlineControllerJobKey)
                .WithIdentity($"{offlineControllerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(backgroudJobOptions.OfflineCheckerIntervalSeconds)
                .RepeatForever()));

            var outboxKey = new JobKey(nameof(OutboxMessageProcessorJob));
            opts.AddJob<OutboxMessageProcessorJob>(jobOpts =>
                jobOpts.WithIdentity(outboxKey));
            opts.AddTrigger(triggerOpts => triggerOpts
                .ForJob(outboxKey)
                .WithIdentity($"{outboxKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(backgroudJobOptions.OutboxProcessorIntervalSeconds)
                .RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        RabbitMqOptions rabbitOgtions = rabbitSection.Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException(DiErrors.RabbitMqConfiguration);

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
