using MediatR;
using Npgsql;
using Respawn;

namespace Control.Infrastructure.IntegrationTests.Infrastructure;

[Collection("IntegrationTestCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly ControlDbContext DbContext;
    protected readonly TestUserContext UserContext;
    private readonly string _dbConnectionString;

    private Respawner _respawner = default!;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ControlDbContext>();
        UserContext = factory.Services.GetRequiredService<TestUserContext>();

        _dbConnectionString = DbContext.Database.GetConnectionString()!;
    }

    protected T GetRequiredService<T>() where T : notnull =>
        _scope.ServiceProvider.GetRequiredService<T>();

    public async Task InitializeAsync()
    {
        UserContext.UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        UserContext.IsAuthenticated = true;

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
