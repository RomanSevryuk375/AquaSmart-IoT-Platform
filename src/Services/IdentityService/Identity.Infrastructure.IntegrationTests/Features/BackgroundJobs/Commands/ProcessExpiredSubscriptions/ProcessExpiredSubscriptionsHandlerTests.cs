using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using BuildingBlocks.IntegrationEvents.Events.Users;
using IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;
using IdentityService.Domain.Events;
using MassTransit.Testing;

namespace Identity.Infrastructure.IntegrationTests.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;

public class ProcessExpiredSubscriptionsHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldDowngradeExpiredSubscriptionsAndPublishEvents()
    {
        // Arrange
        Subscription freeSubscription = new SubscriptionBuilder()
            .WithId(Guid.Parse(SubscriptionType.Free))
            .WithName("Free Plan")
            .Build();

        Subscription proSubscription = new SubscriptionBuilder()
            .WithId(Guid.Parse(SubscriptionType.Professional))
            .WithName("Professional Plan")
            .Build();

        await DbContext.Subscriptions.AddRangeAsync(freeSubscription, proSubscription);
        await DbContext.SaveChangesAsync();

        User expiredUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Expired User")
            .WithEmail("expired@example.com")
            .WithSubscriptionId(proSubscription.Id)
            .Build();
        expiredUser.SetSubscription(proSubscription.Id, -2);
        expiredUser.ClearDomainEvents();

        User validUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Valid User")
            .WithEmail("valid@example.com")
            .WithSubscriptionId(proSubscription.Id)
            .Build();
        validUser.SetSubscription(proSubscription.Id, 5);
        validUser.ClearDomainEvents();

        await DbContext.Users.AddRangeAsync(expiredUser, validUser);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        ITestHarness harness = GetRequiredService<ITestHarness>();

        var command = new ProcessExpiredSubscriptionsCommand();

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        User? updatedExpiredUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == expiredUser.Id);

        updatedExpiredUser.Should().NotBeNull();
        updatedExpiredUser!.SubscriptionId.Should().Be(Guid.Parse(SubscriptionType.Free));

        User? updatedValidUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == validUser.Id);

        updatedValidUser.Should().NotBeNull();
        updatedValidUser!.SubscriptionId.Should().Be(Guid.Parse(SubscriptionType.Professional));
        updatedValidUser.SubscriptionEndDate.Should().BeCloseTo(validUser.SubscriptionEndDate, TimeSpan.FromSeconds(2));

        // 1. Проверяем создание OutboxMessage
        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().ContainSingle(m => m.Type.Contains(nameof(SubscriptionDowngradedDomainEvent)));
        OutboxMessage outboxMessage = outboxMessages.Single(m => m.Type.Contains(nameof(SubscriptionDowngradedDomainEvent)));
        outboxMessage.Content.Should().Contain(expiredUser.Id.ToString());
        outboxMessage.Content.Should().Contain(Guid.Parse(SubscriptionType.Free).ToString());

        // 2. Обрабатываем OutboxMessage через сервис процессора
        OutboxMessageProcessorService<IdentityDbContext> outboxProcessor =
            GetRequiredService<OutboxMessageProcessorService<IdentityDbContext>>();
        Result processResult = await outboxProcessor.ProcessAsync(CancellationToken.None);
        processResult.IsSuccess.Should().BeTrue();

        // 3. Проверяем публикацию интеграционного события в MassTransit
        bool anyPublished = await harness.Published.Any<SubscriptionDowngradedEvent>();
        anyPublished.Should().BeTrue();

        var publishedEvents = harness.Published.Select<SubscriptionDowngradedEvent>().ToList();
        publishedEvents.Should().ContainSingle(x => x.Context.Message.UserId == expiredUser.Id &&
                                                    x.Context.Message.NewSubscriptionId == Guid.Parse(SubscriptionType.Free));
    }
}
