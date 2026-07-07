using System.Net.Http.Headers;
using Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace Control.API.E2ETests.Infrastructure;

[Collection("E2ETestCollection")]
public abstract class BaseE2ETest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly ControlDbContext DbContext;
    protected readonly E2ETestWebAppFactory Factory;

    private readonly IServiceScope _scope;
    private readonly string _dbConnectionString;
    private Respawner _respawner = default!;

    protected BaseE2ETest(E2ETestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ControlDbContext>();
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
