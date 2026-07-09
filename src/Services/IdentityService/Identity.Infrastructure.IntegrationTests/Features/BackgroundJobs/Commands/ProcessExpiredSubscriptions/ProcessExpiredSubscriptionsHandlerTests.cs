using Contracts.Events.UserEvents;
using IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;
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

        DbContext.Subscriptions.AddRange(freeSubscription, proSubscription);
        await DbContext.SaveChangesAsync();

        User expiredUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Expired User")
            .WithEmail("expired@example.com")
            .WithSubscriptionId(proSubscription.Id)
            .Build();
        expiredUser.SetSubscription(proSubscription.Id, -2);

        User validUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Valid User")
            .WithEmail("valid@example.com")
            .WithSubscriptionId(proSubscription.Id)
            .Build();
        validUser.SetSubscription(proSubscription.Id, 5);

        DbContext.Users.AddRange(expiredUser, validUser);
        await DbContext.SaveChangesAsync();

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

        bool anyPublished = await harness.Published.Any<SubscriptionDowngradedEvent>();
        anyPublished.Should().BeTrue();

        var publishedEvents = harness.Published.Select<SubscriptionDowngradedEvent>().ToList();
        publishedEvents.Should().ContainSingle(x => x.Context.Message.UserId == expiredUser.Id);
    }
}
