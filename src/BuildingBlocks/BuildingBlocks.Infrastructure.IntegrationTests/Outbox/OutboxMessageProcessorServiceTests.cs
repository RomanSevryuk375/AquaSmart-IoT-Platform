using System.Text.Json;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.IntegrationTests.Outbox;

[Collection("PostgresOutbox")]
public class OutboxMessageProcessorServiceTests(PostgresOutboxFixture fixture)
{
    private static async Task ResetOutboxAsync(OutboxTestDbContext dbContext)
    {
        await dbContext.OutboxMessages.ExecuteDeleteAsync();
        dbContext.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ProcessAsync_ShouldProcessValidMessageAndInvokeHandler()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        var tracker = scope.ServiceProvider.GetRequiredService<TestDomainEventTracker>();
        await ResetOutboxAsync(dbContext);

        var domainEvent = new TestDomainEvent(Guid.NewGuid(), "valid-payload");
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tracker.HandledEvents.Should().ContainSingle(e => e.AggregateId == domainEvent.AggregateId);

        OutboxMessage? processed = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        processed.Should().NotBeNull();
        processed!.ProcessedOnUtc.Should().NotBeNull();
        processed.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenTypeIsUnresolvable()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = "NonExistentNamespace.NonExistentClass, NonExistentAssembly",
            Content = "{}"
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        OutboxMessage? processed = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        processed.Should().NotBeNull();
        processed!.ProcessedOnUtc.Should().NotBeNull();
        processed.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenContentIsInvalidJson()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = "[]"
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        OutboxMessage? processed = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        processed.Should().NotBeNull();
        processed!.ProcessedOnUtc.Should().NotBeNull();
        processed.Error.Should().Contain("JSON deserialization error");
    }

    [Fact]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenContentDeserializesToNull()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = "null"
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        OutboxMessage? processed = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        processed.Should().NotBeNull();
        processed!.ProcessedOnUtc.Should().NotBeNull();
        processed.Error.Should().Contain("Content is not an IDomainEvent");
    }

    [Fact]
    public async Task ProcessAsync_ShouldSkipMessage_WhenNextRetryIsInFuture()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var domainEvent = new TestDomainEvent(Guid.NewGuid(), "future-retry");
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            NextRetryOnUtc = DateTime.UtcNow.AddMinutes(10),
            RetryCount = 1,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        OutboxMessage? messageInDb = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        messageInDb.Should().NotBeNull();
        messageInDb!.ProcessedOnUtc.Should().BeNull();
        messageInDb.RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_ShouldProcessMessage_WhenNextRetryIsInPast()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var domainEvent = new TestDomainEvent(Guid.NewGuid(), "past-retry");
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            NextRetryOnUtc = DateTime.UtcNow.AddMinutes(-1),
            RetryCount = 1,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        OutboxMessage? messageInDb = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        messageInDb.Should().NotBeNull();
        messageInDb!.ProcessedOnUtc.Should().NotBeNull();
        messageInDb.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WhenHandlerThrows_SchedulesRetryAndMarksFailedAtMaxRetries()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider(opts => opts.MaxRetries = 2);
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        var tracker = scope.ServiceProvider.GetRequiredService<TestDomainEventTracker>();
        tracker.ShouldThrow = true;
        await ResetOutboxAsync(dbContext);

        var domainEvent = new TestDomainEvent(Guid.NewGuid(), "failing-event");
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act - Attempt 1
        await service.ProcessAsync(CancellationToken.None);

        OutboxMessage? afterFirstAttempt = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        afterFirstAttempt.Should().NotBeNull();
        afterFirstAttempt!.RetryCount.Should().Be(1);
        afterFirstAttempt.ProcessedOnUtc.Should().BeNull();
        afterFirstAttempt.NextRetryOnUtc.Should().NotBeNull();
        afterFirstAttempt.Error.Should().Contain("Simulated handler failure.");

        // Make retry eligible immediately for attempt 2
        await dbContext.OutboxMessages
            .Where(m => m.Id == outboxMessage.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.NextRetryOnUtc, DateTime.UtcNow.AddSeconds(-1)));

        // Act - Attempt 2 (hits MaxRetries = 2)
        await service.ProcessAsync(CancellationToken.None);

        // Assert
        OutboxMessage? afterSecondAttempt = await dbContext.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        afterSecondAttempt.Should().NotBeNull();
        afterSecondAttempt!.RetryCount.Should().Be(2);
        afterSecondAttempt.ProcessedOnUtc.Should().NotBeNull();
        afterSecondAttempt.Error.Should().Contain("Simulated handler failure.");
    }

    [Fact]
    public async Task CleanupAsync_ShouldDeleteProcessedMessagesOlderThanRetentionDays()
    {
        // Arrange
        await using ServiceProvider sp = fixture.BuildServiceProvider();
        using IServiceScope scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await ResetOutboxAsync(dbContext);

        var oldProcessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-10),
            ProcessedOnUtc = DateTime.UtcNow.AddDays(-8),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        var recentProcessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-1),
            ProcessedOnUtc = DateTime.UtcNow.AddMinutes(-30),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        var unprocessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-10),
            ProcessedOnUtc = null,
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        await dbContext.OutboxMessages.AddRangeAsync(oldProcessedMessage, recentProcessedMessage, unprocessedMessage);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var service = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessorService<OutboxTestDbContext>>();

        // Act
        Result result = await service.CleanupAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        List<OutboxMessage> remaining = await dbContext.OutboxMessages.AsNoTracking().ToListAsync();
        remaining.Should().NotContain(m => m.Id == oldProcessedMessage.Id);
        remaining.Should().Contain(m => m.Id == recentProcessedMessage.Id);
        remaining.Should().Contain(m => m.Id == unprocessedMessage.Id);
    }
}
