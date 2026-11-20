using System.Text.Json;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using IdentityService.Domain.Events;

namespace Identity.Infrastructure.IntegrationTests.BackgroundJobs;

public class OutboxMessageProcessorServiceTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
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
}
