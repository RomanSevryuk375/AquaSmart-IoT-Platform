// Ignore Spelling: Tg

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Persistence;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Notification.Infrastructure.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:16-alpine";
    private const string DatabaseName = "notification_test_db";
    private const string Username = "postgres";
    private const string Password = "postgres";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        .Build();

    public INotificationProvider EmailProviderMock { get; } = Substitute.For<INotificationProvider>();
    public INotificationProvider TgProviderMock { get; } = Substitute.For<INotificationProvider>();

    public async Task InitializeAsync() => await _dbContainer.StartAsync();

    public new async Task DisposeAsync() => await _dbContainer.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:NotificationDbContext", _dbContainer.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? massTransitHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "MassTransitHostedService");
            if (massTransitHostedService != null)
            {
                services.Remove(massTransitHostedService);
            }

            ServiceDescriptor? quartzHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "QuartzHostedService");
            if (quartzHostedService != null)
            {
                services.Remove(quartzHostedService);
            }

            ServiceDescriptor? migrationHostedService = services.FirstOrDefault(d =>
                d.ImplementationType?.Name == "DatabaseMigrationService");
            if (migrationHostedService != null)
            {
                services.Remove(migrationHostedService);
            }

            var descriptors = services.Where(d => d.ServiceType == typeof(INotificationProvider)).ToList();
            foreach (ServiceDescriptor? descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<INotificationProvider>(EmailProviderMock);
            services.AddSingleton<INotificationProvider>(TgProviderMock);

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
            NotificationDbContext context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            context.Database.Migrate();
        });
    }
}

public sealed class TestUserContext : IUserContext
{
    public Guid UserId { get; set; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public bool IsAuthenticated { get; set; } = true;
}
