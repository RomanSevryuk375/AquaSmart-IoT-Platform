using Identity.TestShared.Constants;
using IdentityService.Application.Features.Profile.Commands.UpdateProfile;
using IdentityService.Infrastructure.Persistence.Outbox;

namespace Identity.Infrastructure.IntegrationTests.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldUpdateProfileAndCreateOutboxMessage_WhenCommandIsValid()
    {
        // Arrange
        Subscription subscription = new SubscriptionBuilder()
            .WithId(IdentityTestConstants.SubscriptionId)
            .Build();

        User user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Old Name")
            .WithEmail("alice@aquasmart.com")
            .WithPhoneNumber("+375295554433")
            .WithSubscriptionId(subscription.Id)
            .Build();

        DbContext.Subscriptions.Add(subscription);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        UserContext.UserId = user.Id;
        UserContext.IsAuthenticated = true;

        var command = new UpdateProfileCommand
        {
            Name = "New Name",
            PhoneNumber = "+375296667788"
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        User? updatedUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.Name.Value.Should().Be("New Name");
        updatedUser.PhoneNumber.Should().Be("+375296667788");

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().ContainSingle(m => m.Type.Contains("UserUpdatedDomainEvent"));
        OutboxMessage outboxMessage = outboxMessages.Single(m => m.Type.Contains("UserUpdatedDomainEvent"));
        outboxMessage.Content.Should().Contain("New Name");
        outboxMessage.Content.Should().Contain("+375296667788");
    }
}
