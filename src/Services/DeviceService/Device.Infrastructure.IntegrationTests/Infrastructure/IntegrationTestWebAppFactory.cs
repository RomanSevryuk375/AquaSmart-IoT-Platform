using BuildingBlocks.TestShared;
using MassTransit;

namespace Device.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : BaseIntegrationTestWebAppFactory<Program, DeviceDbContext>
{
    protected override string GetDbConnectionStringName() => "ConnectionStrings:DeviceDbContext";

    protected override void ConfigureMassTransit(IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));
    }
}
