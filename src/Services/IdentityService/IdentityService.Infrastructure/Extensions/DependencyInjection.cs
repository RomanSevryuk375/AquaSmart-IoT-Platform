// Ignore Spelling: Mq

using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.IntegrationEvents;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.BackgroundJobs;
using IdentityService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace IdentityService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddPostgresDbContext<IdentityDbContext>(configuration)
                       .AddDapper<IdentityDbContext>()
                       .AddRepositories()
                       .AddRabbitMq(configuration)
                       .AddOutboxProcessorQuartzJob<IdentityDbContext>(configuration)
                       .AddQuartzJobs()
                       .AddUserContext()
                       .AddCache(configuration);
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration) =>
        services.AddGlobalMessaging(configuration);

    private static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartzWithHostedService(opts =>
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
        });

        return services;
    }
}
