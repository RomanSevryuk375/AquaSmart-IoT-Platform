// Ignore Spelling: Mq

using Contracts.Options;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.BackgroundJobs;
using IdentityService.Infrastructure.Factories;
using IdentityService.Infrastructure.Persistence.Interceptors;
using IdentityService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace IdentityService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddRepositories(configuration)
                       .AddRabbitMq(configuration)
                       .AddQuartzJobs();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        string? connectionString = configuration.GetConnectionString(nameof(IdentityDbContext));
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            ConvertDomainEventsToOutboxMessagesInterceptor interceptor =
                sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(interceptor);
        });
        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHttpContextAccessor();

        services.AddHostedService<DatabaseMigrationService>();

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        RabbitMqOptions rabbitOptions = rabbitSection.Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.Configure<RabbitMqOptions>(rabbitSection);

        services.AddMassTransit(busConfigurations =>
        {
            busConfigurations.SetKebabCaseEndpointNameFormatter();

            busConfigurations.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(rabbitOptions.Host), h =>
                {
                    h.Username(rabbitOptions.UserName);
                    h.Password(rabbitOptions.Password);
                });

                configurator.ConfigureEndpoints(context);
            });
        });

        services.AddHealthChecks().AddRabbitMQ(new Uri(rabbitOptions.Host));

        return services;
    }

    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(opts =>
        {
            var incorrectTokenCheckerJobKey = new JobKey(nameof(IncorrectTokenCheckerJob));
            opts.AddJob<IncorrectTokenCheckerJob>(jobOpts => jobOpts.WithIdentity(incorrectTokenCheckerJobKey));
            opts.AddTrigger(triggerOptions => triggerOptions
                .ForJob(incorrectTokenCheckerJobKey)
                .WithIdentity($"{incorrectTokenCheckerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()));

            var subscriptionExpiredCheckerJobKey = new JobKey(nameof(SubscriptionExpiredCheckerJob));
            opts.AddJob<SubscriptionExpiredCheckerJob>(jobOpts => jobOpts.WithIdentity(subscriptionExpiredCheckerJobKey));
            opts.AddTrigger(triggerOptions => triggerOptions
                .ForJob(subscriptionExpiredCheckerJobKey)
                .WithIdentity($"{subscriptionExpiredCheckerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));

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

internal sealed class DatabaseMigrationService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
