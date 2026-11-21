using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Infrastructure.Data.Interceptors;
using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.Infrastructure.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Infrastructure.IntegrationTests.Outbox;

public sealed record TestDomainEvent(Guid AggregateId, string Payload) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public sealed class TestAggregate : AggregateRoot, IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private TestAggregate() { }

    public static TestAggregate Create(string name)
    {
        var aggregate = new TestAggregate
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
        aggregate.RaiseEvent(new TestDomainEvent(aggregate.Id, name));
        return aggregate;
    }
}

[CollectionDefinition("PostgresOutbox")]
public sealed class PostgresOutboxCollection : ICollectionFixture<PostgresOutboxFixture>;

public sealed class TestDomainEventTracker
{
    public List<TestDomainEvent> HandledEvents { get; } = [];
    public bool ShouldThrow { get; set; }
}

public sealed class TestDomainEventHandler(TestDomainEventTracker tracker) : INotificationHandler<TestDomainEvent>
{
    public Task Handle(TestDomainEvent notification, CancellationToken cancellationToken)
    {
        if (tracker.ShouldThrow)
        {
            throw new InvalidOperationException("Simulated handler failure.");
        }

        tracker.HandledEvents.Add(notification);
        return Task.CompletedTask;
    }
}

public sealed class OutboxTestDbContext(DbContextOptions<OutboxTestDbContext> options) : DbContext(options)
{
    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureOutbox();
        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.ToTable("test_aggregates");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
        });
    }
}

public sealed class PostgresOutboxFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("outbox_test_db")
        .WithUsername("postgres")
#pragma warning disable S2068
        .WithPassword("postgres")
#pragma warning restore S2068
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using ServiceProvider sp = BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        OutboxTestDbContext dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public ServiceProvider BuildServiceProvider(Action<OutboxOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDomainEventTracker>();
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddDbContext<OutboxTestDbContext>((sp, options) =>
        {
            ConvertDomainEventsToOutboxMessagesInterceptor interceptor =
                sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();
            options.UseNpgsql(ConnectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(interceptor);
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TestDomainEventHandler).Assembly));

        var outboxOptions = new OutboxOptions();
        configureOptions?.Invoke(outboxOptions);
        services.AddSingleton(Options.Create(outboxOptions));
        services.AddLogging();
        services.AddScoped(sp => new OutboxMessageProcessorService<OutboxTestDbContext>(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxMessageProcessorService<OutboxTestDbContext>>.Instance,
            sp.GetRequiredService<IOptions<OutboxOptions>>()));

        return services.BuildServiceProvider();
    }
}
