using System.Text.Json;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using IdentityService.Domain.Events;

namespace Identity.Infrastructure.IntegrationTests.BackgroundJobs;

public class OutboxMessageProcessorServiceTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "")]
    public async Task ProcessAsync_ShouldProcessValidMessageSuccessfully()
    {
        // Arrange
        var domainEvent = new UserUpdatedDomainEvent
        {
            UserId = Guid.NewGuid(),
            Name = "Success User",
            PhoneNumber = "+375291112233"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? processedMessage = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOnUtc.Should().NotBeNull();
        processedMessage.Error.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenTypeIsUnresolvable()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = "NonExistentNamespace.NonExistentClass, NonExistentAssembly",
            Content = "{}"
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? processedMessage = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOnUtc.Should().NotBeNull();
        processedMessage.Error.Should().NotBeNull();

        processedMessage.Error.Should().Contain("not found");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenContentIsInvalidJson()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = "[]"
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? processedMessage = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOnUtc.Should().NotBeNull();
        processedMessage.Error.Should().NotBeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ProcessAsync_ShouldMarkAsPoisonMessage_WhenContentDeserializesToNull()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = "null"
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? processedMessage = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedOnUtc.Should().NotBeNull();
        processedMessage.Error.Should().NotBeNull();

        processedMessage.Error.Should().Contain("Content is not an IDomainEvent");
    }

    [Fact]
    public async Task ProcessAsync_ShouldSkipMessage_WhenNextRetryIsInFuture()
    {
        // Arrange
        var domainEvent = new UserUpdatedDomainEvent
        {
            UserId = Guid.NewGuid(),
            Name = "Future Retry User",
            PhoneNumber = "+375291112233"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            NextRetryOnUtc = DateTime.UtcNow.AddMinutes(10),
            RetryCount = 1,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? messageInDb = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        messageInDb.Should().NotBeNull();
        messageInDb!.ProcessedOnUtc.Should().BeNull();
        messageInDb.RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_ShouldProcessMessage_WhenNextRetryIsInPast()
    {
        // Arrange
        var domainEvent = new UserUpdatedDomainEvent
        {
            UserId = Guid.NewGuid(),
            Name = "Past Retry User",
            PhoneNumber = "+375291112233"
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            NextRetryOnUtc = DateTime.UtcNow.AddMinutes(-1),
            RetryCount = 1,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent)
        };

        await DbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.ProcessAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        OutboxMessage? messageInDb = await DbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);

        messageInDb.Should().NotBeNull();
        messageInDb!.ProcessedOnUtc.Should().NotBeNull();
        messageInDb.Error.Should().BeNull();
    }

    [Fact]
    public async Task CleanupAsync_ShouldDeleteProcessedMessagesOlderThanRetentionDays()
    {
        // Arrange
        var oldProcessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-10),
            ProcessedOnUtc = DateTime.UtcNow.AddDays(-8),
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        var recentProcessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-1),
            ProcessedOnUtc = DateTime.UtcNow.AddMinutes(-30),
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        var unprocessedMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddDays(-10),
            ProcessedOnUtc = null,
            Type = typeof(UserUpdatedDomainEvent).AssemblyQualifiedName!,
            Content = "{}"
        };

        await DbContext.Set<OutboxMessage>().AddRangeAsync(oldProcessedMessage, recentProcessedMessage, unprocessedMessage);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        OutboxMessageProcessorService<IdentityDbContext> service = GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();

        // Act
        Result result = await service.CleanupAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<OutboxMessage> remaining = await DbContext.Set<OutboxMessage>().AsNoTracking().ToListAsync();

        remaining.Should().NotContain(m => m.Id == oldProcessedMessage.Id);
        remaining.Should().Contain(m => m.Id == recentProcessedMessage.Id);
        remaining.Should().Contain(m => m.Id == unprocessedMessage.Id);
    }
}
