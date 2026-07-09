using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Telemetry.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Telemetry.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:16-alpine";
    private const string DatabaseName = "telemetry_test_db";
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

        builder.UseSetting($"ConnectionStrings:{nameof(TelemetryDbContext)}", _dbContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            var massTransitDescriptors = services.Where(d =>
                d.ServiceType.Namespace?.StartsWith("MassTransit") == true ||
                d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true).ToList();

            foreach (ServiceDescriptor descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            ServiceDescriptor? quartzHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "QuartzHostedService");
            if (quartzHostedService != null)
            {
                services.Remove(quartzHostedService);
            }

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            TelemetryDbContext context = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
            context.Database.Migrate();
        });
    }
}
