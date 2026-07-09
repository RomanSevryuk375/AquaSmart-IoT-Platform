using System.Net.Http.Headers;
using Identity.Infrastructure.IntegrationTests.Infrastructure;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace Identity.API.E2ETests.Infrastructure;

[Collection("E2ETestCollection")]
public abstract class BaseE2ETest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly IdentityDbContext DbContext;
    protected readonly UserManager<User> UserManager;
    protected readonly TestUserContext UserContext;
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
        DbContext = _scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        UserManager = _scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        UserContext = factory.Services.GetRequiredService<TestUserContext>();
        _dbConnectionString = DbContext.Database.GetConnectionString()!;
    }

    public async Task InitializeAsync()
    {
        UserContext.UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        UserContext.IsAuthenticated = true;

        using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory", "subscriptions"]
        });

        await _respawner.ResetAsync(connection);
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
