using BuildingBlocks.Infrastructure.Data.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.IntegrationTests.Outbox;

[Collection("PostgresOutbox")]
public class OutboxInterceptorTests(PostgresOutboxFixture fixture)
{
    [Fact]
    public async Task SaveChangesAsync_WhenAggregateRaisesDomainEvent_PersistsOutboxMessageAndClearsDomainEvents()
    {
        // Arrange
        await using var sp = fixture.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxTestDbContext>();
        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        var aggregate = TestAggregate.Create("alpha");

        // Act
        dbContext.Aggregates.Add(aggregate);
        await dbContext.SaveChangesAsync();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();

        List<OutboxMessage> messages = await dbContext.OutboxMessages.AsNoTracking().ToListAsync();
        messages.Should().ContainSingle();
        messages[0].Type.Should().Contain(nameof(TestDomainEvent));
        messages[0].ProcessedOnUtc.Should().BeNull();
        messages[0].Error.Should().BeNull();
        messages[0].Content.Should().Contain(aggregate.Id.ToString());
    }
}
