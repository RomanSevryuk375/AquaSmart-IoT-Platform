using BuildingBlocks.Domain.Abstractions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.TestShared;

public abstract class BaseIntegrationTestWebAppFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    private const string PostgresImage = "postgres:16-alpine";
    private const string DatabaseName = "test_db";
    private const string Username = "postgres";
#pragma warning disable S2068 // Hard-coded credentials are safe in tests for Testcontainers
    private const string Password = "postgres";
#pragma warning restore S2068 // Hard-coded credentials are safe in tests for Testcontainers

    protected readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder(PostgresImage)
        .WithImage(PostgresImage)
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        .Build();

    public async Task InitializeAsync() => await DbContainer.StartAsync();

    public new async Task DisposeAsync() => await DbContainer.DisposeAsync();

    protected abstract string GetDbConnectionStringName();

    protected virtual void ConfigureMassTransit(IServiceCollection services)
    {
        var massTransitDescriptors = services.Where(d =>
            d.ServiceType.Namespace?.StartsWith("MassTransit") is true ||
            d.ImplementationType?.Namespace?.StartsWith("MassTransit") is true).ToList();

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
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(GetDbConnectionStringName(), DbContainer.GetConnectionString());

        builder.UseSetting("JwtOptions:SecretKey", "test-secret-key-must-be-at-least-32-characters-long");
        builder.UseSetting("JwtOptions:Issuer", "AquaSmart.Identity");
        builder.UseSetting("JwtOptions:Audience", "AquaSmart.Gateway");
        builder.UseSetting("JwtOptions:ExpiresHours", "12");

        builder.ConfigureServices(services =>
        {
            ConfigureMassTransit(services);

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
            TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            context.Database.Migrate();
        });
    }
}
