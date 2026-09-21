using BuildingBlocks.Domain.Results;
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
        User user = await CreateUserWithSubscriptionAsync("Alice Smith", "alice@aquasmart.com", "+375295554433");

        var query = new GetMyProfileQuery(){
            UserId = UserContext.UserId
        };

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

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var query = new GetMyProfileQuery
        {
            UserId = Guid.NewGuid()
        };

        // Act
        Result<UserProfileResponseDto> result = await Sender.Send(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
