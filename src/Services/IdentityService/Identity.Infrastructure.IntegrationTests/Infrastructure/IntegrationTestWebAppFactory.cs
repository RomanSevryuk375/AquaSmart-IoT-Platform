using IdentityService.Domain.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Identity.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:16-alpine";
    private const string DatabaseName = "identity_test_db";
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

        builder.UseSetting("ConnectionStrings:IdentityDbContext", _dbContainer.GetConnectionString());
        builder.UseSetting("JwtOptions:SecretKey", "super-secret-key-must-be-at-least-32-characters-long");
        builder.UseSetting("JwtOptions:Issuer", "AquaSmart.Identity");
        builder.UseSetting("JwtOptions:Audience", "AquaSmart.Gateway");
        builder.UseSetting("JwtOptions:ExpiresHours", "12");
        builder.UseSetting("RabbitMqOptions:Host", "amqp://localhost:5672");
        builder.UseSetting("RabbitMqOptions:UserName", "guest");
        builder.UseSetting("RabbitMqOptions:Password", "guest");

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
                x.AddDelayedMessageScheduler();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddMassTransitTestHarness();

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
            IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            context.Database.Migrate();
        });
    }
}

public sealed class TestUserContext : IUserContext
{
    public Guid UserId { get; set; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public bool IsAuthenticated { get; set; } = true;
}
