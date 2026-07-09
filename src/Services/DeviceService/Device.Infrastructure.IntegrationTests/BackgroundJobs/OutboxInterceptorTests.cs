using Device.Domain.Events.ControllerEvents;
using Device.Infrastructure.Persistence.Outbox;

namespace Device.Infrastructure.IntegrationTests.BackgroundJobs;

public class OutboxInterceptorTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task SaveChanges_WhenEntityHasDomainEvents_CreatesOutboxMessageInDatabase()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        controller.ClearDomainEvents();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        // Act
        controller.SetOffline();
        await DbContext.SaveChangesAsync();

        // Assert
        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages.AsNoTracking().ToListAsync();

        outboxMessages.Should().ContainSingle();

        OutboxMessage message = outboxMessages[0];

        message.Type.Should().Contain(nameof(ControllerNotOnlineDomainEvent));
        message.ProcessedOnUtc.Should().BeNull();
        message.Error.Should().BeNull();
        message.Content.Should().Contain(controller.Id.ToString());
    }
}
