using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;

namespace BuildingBlocks.Infrastructure.Data.Outbox;

public static class OutboxExtension
{
    public static IServiceCollection AddOutboxProcessorQuartzJob<TDbContext>(
        this IServiceCollection services) where TDbContext : DbContext
        => services.AddOutboxProcessorQuartzJobCore<TDbContext>(null);

    public static IServiceCollection AddOutboxProcessorQuartzJob<TDbContext>(
        this IServiceCollection services,
        int intervalSeconds) where TDbContext : DbContext
    {
        return services.AddOutboxProcessorQuartzJobCore<TDbContext>(options =>
        {
            options.IntervalSeconds = intervalSeconds;
        });
    }

    public static IServiceCollection AddOutboxProcessorQuartzJob<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration) where TDbContext : DbContext
    {
        var options = new OutboxOptions();
        IConfigurationSection section = configuration.GetSection(OutboxOptions.SectionName);
        if (section.Exists())
        {
            section.Bind(options);
        }
        else
        {
            IConfigurationSection bgSection = configuration.GetSection("BackgroundJobs");
            if (bgSection.Exists())
            {
                int? outboxInterval = bgSection.GetValue<int?>("OutboxProcessorIntervalSeconds");
                if (outboxInterval.HasValue)
                {
                    options.IntervalSeconds = outboxInterval.Value;
                }
            }
        }

        return services.AddOutboxProcessorQuartzJobCore<TDbContext>(opt =>
        {
            opt.IntervalSeconds = options.IntervalSeconds;
            opt.BatchSize = options.BatchSize;
            opt.MaxRetries = options.MaxRetries;
            opt.RetentionDays = options.RetentionDays;
            opt.CleanupIntervalHours = options.CleanupIntervalHours;
        });
    }

    public static IServiceCollection AddOutboxProcessorQuartzJob<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions> configure) where TDbContext : DbContext => services.AddOutboxProcessorQuartzJobCore<TDbContext>(configure);

    private static IServiceCollection AddOutboxProcessorQuartzJobCore<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions>? configure) where TDbContext : DbContext
    {
        var options = new OutboxOptions();
        configure?.Invoke(options);

        services.AddSingleton(Options.Create(options));
        services.AddScoped<OutboxMessageProcessorService<TDbContext>>();

        services.AddQuartz(opts =>
        {
            var outboxKey = new JobKey($"OutboxMessageProcessorJob-{typeof(TDbContext).Name}");
            opts.AddJob<OutboxMessageProcessorJob<TDbContext>>(jobOpts => jobOpts.WithIdentity(outboxKey));
            opts.AddTrigger(trigger => trigger
                .ForJob(outboxKey)
                .WithIdentity($"{outboxKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(options.IntervalSeconds).RepeatForever()));

            var cleanupKey = new JobKey($"OutboxCleanupJob-{typeof(TDbContext).Name}");
            opts.AddJob<OutboxCleanupJob<TDbContext>>(jobOpts => jobOpts.WithIdentity(cleanupKey));
            opts.AddTrigger(trigger => trigger
                .ForJob(cleanupKey)
                .WithIdentity($"{cleanupKey}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(options.CleanupIntervalHours).RepeatForever()));
        });

        if (!services.Any(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType?.Name == "QuartzHostedService"))
        {
            services.AddQuartzHostedService(hostOpts => hostOpts.WaitForJobsToComplete = true);
        }

        return services;
    }
}
