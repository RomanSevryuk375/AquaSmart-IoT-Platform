using MediatR;
using Npgsql;
using Respawn;

namespace Device.Infrastructure.IntegrationTests.Infrastructure;

[Collection("IntegrationTestCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly SystemDbContext DbContext;
    private readonly string _dbConnectionString;

    private Respawner _respawner = default!;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<SystemDbContext>();

        _dbConnectionString = DbContext.Database.GetConnectionString()!;
    }

    public async Task InitializeAsync()
    {
        using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });

        await _respawner.ResetAsync(connection);
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
