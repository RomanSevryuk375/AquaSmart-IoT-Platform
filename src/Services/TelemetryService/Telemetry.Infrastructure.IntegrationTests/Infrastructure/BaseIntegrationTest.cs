using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.Infrastructure.IntegrationTests.Infrastructure;

[Collection("IntegrationTestCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly TelemetryDbContext DbContext;
    private readonly string _dbConnectionString;

    private Respawner _respawner = default!;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        _dbConnectionString = DbContext.Database.GetConnectionString()!;
    }

    protected T GetRequiredService<T>() where T : notnull =>
        _scope.ServiceProvider.GetRequiredService<T>();

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
