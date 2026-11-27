using BuildingBlocks.TestShared;
using Microsoft.AspNetCore.TestHost;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Persistence;
using NSubstitute;
using MassTransit;

namespace Notification.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : BaseIntegrationTestWebAppFactory<Program, NotificationDbContext>
{
    protected override string GetDbConnectionStringName() => "ConnectionStrings:NotificationDbContext";

    public IEmailProvider EmailProviderMock { get; } = Substitute.For<IEmailProvider>();
    public ITgProvider TgProviderMock { get; } = Substitute.For<ITgProvider>();

    protected override void ConfigureMassTransit(IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x => x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); }));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<BuildingBlocks.Infrastructure.Data.Outbox.OutboxMessageProcessorService<NotificationDbContext>>();

            ServiceDescriptor? migrationHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "DatabaseMigrationService");
            if (migrationHostedService != null)
            {
                services.Remove(migrationHostedService);
            }

            var tgDescriptors = services.Where(d => d.ServiceType == typeof(ITgProvider)).ToList();
            foreach (ServiceDescriptor? descriptor in tgDescriptors)
            {
                services.Remove(descriptor);
            }

            var emailDescriptors = services.Where(d => d.ServiceType == typeof(IEmailProvider)).ToList();
            foreach (ServiceDescriptor? descriptor in emailDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IEmailProvider>(EmailProviderMock);
            services.AddSingleton<ITgProvider>(TgProviderMock);
        });
    }
}
