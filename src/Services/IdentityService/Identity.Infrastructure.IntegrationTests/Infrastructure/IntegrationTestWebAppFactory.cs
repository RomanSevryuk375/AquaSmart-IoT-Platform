using BuildingBlocks.IntegrationTests;
using Identity.TestShared.Helpers;
using MassTransit;
using Microsoft.AspNetCore.TestHost;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Identity.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : BaseIntegrationTestWebAppFactory<Program, IdentityDbContext>
{
    protected override string GetDbConnectionStringName() => "ConnectionStrings:IdentityDbContext";

    protected override void ConfigureMassTransit(IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x => x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); }));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("TelegramBotOptions:Name", HmacTestHelper.TestBotName);
        builder.UseSetting("TelegramBotOptions:BotSecretKey", HmacTestHelper.TestBotSecretKey);

        builder.ConfigureTestServices(services =>
        {
            services.Configure<IdentityService.Application.Options.TelegramBotOptions>(opts =>
            {
                opts.Name = HmacTestHelper.TestBotName;
                opts.BotSecretKey = HmacTestHelper.TestBotSecretKey;
            });
            services.AddScoped<BuildingBlocks.Infrastructure.Data.Outbox.OutboxMessageProcessorService<IdentityDbContext>>();
            services.AddFusionCache()
                .WithDefaultEntryOptions(new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(10) })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer());
        });
    }
}
