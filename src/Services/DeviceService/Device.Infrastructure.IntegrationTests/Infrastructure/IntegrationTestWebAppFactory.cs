using Device.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Device.Infrastructure.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:16-alpine";
    private const string DatabaseName = "device_test_db";
    private const string Username = "postgres";
    private const string Password = "postgres";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgresImage)
        .WithImage(PostgresImage)
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        .Build();

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:SystemDbContext", _dbContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? massTransitHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "MassTransitHostedService");

            if (massTransitHostedService != null)
            {
                services.Remove(massTransitHostedService);
            }

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            SystemDbContext context = scope.ServiceProvider.GetRequiredService<SystemDbContext>();

            context.Database.Migrate();
        });
    }
}
