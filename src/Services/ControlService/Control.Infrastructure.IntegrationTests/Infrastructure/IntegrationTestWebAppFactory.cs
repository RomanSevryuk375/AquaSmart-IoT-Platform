using Control.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Control.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
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

        builder.UseSetting("ConnectionStrings:ControlDbContext", _dbContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            var massTransitDescriptors = services.Where(d =>
                d.ServiceType.Namespace?.StartsWith("MassTransit") == true ||
                d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true).ToList();

            foreach (ServiceDescriptor? descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            });

            ServiceDescriptor? quartzHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "QuartzHostedService");
            if (quartzHostedService != null)
            {
                services.Remove(quartzHostedService);
            }

            ServiceDescriptor? userContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IUserContext));
            if (userContextDescriptor != null)
            {
                services.Remove(userContextDescriptor);
            }

            services.AddSingleton<TestUserContext>();
            services.AddTransient<IUserContext>(sp =>
                sp.GetRequiredService<TestUserContext>());

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            ControlDbContext context = scope.ServiceProvider.GetRequiredService<ControlDbContext>();
            context.Database.Migrate();
        });
    }
}

public sealed class TestUserContext : Control.Application.Interfaces.IUserContext
{
    public Guid UserId { get; set; } = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Defaults to ControlTestConstants.UserId
    public bool IsAuthenticated { get; set; } = true;
}
