using Contracts.Options;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.BackgroundJobs;
using IdentityService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace IdentityService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        var connectionString = configuration.GetConnectionString(nameof(IdentityDbContext));

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddHealthChecks().AddNpgSql(connectionString!);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var rabbitOptions = rabbitSection.Get<RabbitMqOptions>()
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
        services.AddQuartz(options =>
        {
            var incorrectTokenCheckerJobKey = new JobKey(nameof(IncorrectTokenCheckerJob));

            var subscriptionExpiredCheckerJobKey = new JobKey(nameof(SubscriptionExpiredCheckerJob));

            options.AddJob<IncorrectTokenCheckerJob>(jobOptions =>
                jobOptions.WithIdentity(incorrectTokenCheckerJobKey));

            options.AddJob<SubscriptionExpiredCheckerJob>(jobOptions =>
                jobOptions.WithIdentity(subscriptionExpiredCheckerJobKey));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(incorrectTokenCheckerJobKey)
                .WithIdentity($"{incorrectTokenCheckerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()));

            options.AddTrigger(triggerOptions => triggerOptions
                .ForJob(subscriptionExpiredCheckerJobKey)
                .WithIdentity($"{subscriptionExpiredCheckerJobKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));
        });

        services.AddQuartzHostedService(hostOptions
            => hostOptions.WaitForJobsToComplete = true);

        return services;
    }
}
