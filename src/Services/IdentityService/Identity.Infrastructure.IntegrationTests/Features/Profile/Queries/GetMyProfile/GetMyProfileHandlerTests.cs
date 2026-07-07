using Identity.TestShared.Constants;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Profile.Queries.GetMyProfile;

namespace Identity.Infrastructure.IntegrationTests.Features.Profile.Queries.GetMyProfile;

public class GetMyProfileHandlerTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        Subscription subscription = new SubscriptionBuilder()
            .WithId(IdentityTestConstants.SubscriptionId)
            .Build();

        User user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("Alice Smith")
            .WithEmail("alice@aquasmart.com")
            .WithPhoneNumber("+375295554433")
            .WithSubscriptionId(subscription.Id)
            .Build();

        DbContext.Subscriptions.Add(subscription);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        UserContext.UserId = user.Id;
        UserContext.IsAuthenticated = true;

        var query = new GetMyProfileQuery();

        // Act
        Result<UserProfileResponseDto> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);
        result.Value.Name.Should().Be(user.Name.Value);
        result.Value.PhoneNumber.Should().Be(user.PhoneNumber);
        result.Value.SubscriptionId.Should().Be(user.SubscriptionId);
    }
}
