using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
using Npgsql;
using NSubstitute;
using Respawn;

namespace Notification.Infrastructure.IntegrationTests.Infrastructure;

[Collection("IntegrationTestCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly NotificationDbContext DbContext;
    protected readonly TestUserContext UserContext;
    protected readonly IntegrationTestWebAppFactory Factory;
    private readonly string _dbConnectionString;

    private Respawner _respawner = default!;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        UserContext = factory.Services.GetRequiredService<TestUserContext>();

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

        UserContext.UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        Factory.EmailProviderMock.ClearReceivedCalls();
        Factory.TgProviderMock.ClearReceivedCalls();
        Factory.PublishEndpointMock.ClearReceivedCalls();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
