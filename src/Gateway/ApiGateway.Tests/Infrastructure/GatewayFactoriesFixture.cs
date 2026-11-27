namespace ApiGateway.Tests.Infrastructure;

public sealed class GatewayFactoriesFixture : IAsyncLifetime
{
    public GatewayTestWebAppFactory Development { get; } = new("Development");
    public GatewayTestWebAppFactory Production { get; } = new("Production");

    public GatewayTestWebAppFactory GetFactory(string environmentName) => environmentName switch
    {
        "Development" => Development,
        "Production" => Production,
        _ => throw new ArgumentOutOfRangeException(nameof(environmentName))
    };

    public Task InitializeAsync()
    {
        _ = Development.Server;
        _ = Production.Server;
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Development.DisposeAsync();
        await Production.DisposeAsync();
    }
}

[CollectionDefinition("GatewayTests")]
public sealed class GatewayTestsCollection : ICollectionFixture<GatewayFactoriesFixture>;
