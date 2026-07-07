using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.E2ETests.Controllers;

public class ProfileControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetMyProfile_Success_ReturnsOkAndProfileData()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(UserContext.UserId)
            .WithEmail("profile.me@example.com")
            .WithSubscriptionId(Guid.Parse(Contracts.Enums.SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        // Act
        HttpResponseMessage response = await Client.GetAsync("api/identity/v1/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserProfileResponseDto? content = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(user.Id);
        content.Email.Should().Be(user.Email);
        content.Name.Should().Be(user.Name.Value);
        content.PhoneNumber.Should().Be(user.PhoneNumber);
        content.SubscriptionId.Should().Be(user.SubscriptionId);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetMyProfile_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("api/identity/v1/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateMyProfile_Success_UpdatesDbAndReturnsNoContent()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(UserContext.UserId)
            .WithEmail("profile.update@example.com")
            .WithSubscriptionId(Guid.Parse(Contracts.Enums.SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new UpdateProfileRequestDto
        {
            Name = "Updated Name",
            PhoneNumber = "+375299998877"
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync("api/identity/v1/profile/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        User? updatedUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.Name.Value.Should().Be(request.Name);
        updatedUser.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateMyProfile_Validation_InvalidPhoneFormat_ReturnsBadRequest()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(UserContext.UserId)
            .WithEmail("profile.update.val@example.com")
            .WithSubscriptionId(Guid.Parse(Contracts.Enums.SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new UpdateProfileRequestDto
        {
            Name = "Updated Name",
            PhoneNumber = "invalid-phone-format"
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync("api/identity/v1/profile/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ChangePassword_Success_ReturnsNoContent()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(UserContext.UserId)
            .WithEmail("profile.pwd@example.com")
            .WithSubscriptionId(Guid.Parse(Contracts.Enums.SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "CurrentPassword123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/profile/password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        User? updatedUser = await UserManager.FindByIdAsync(user.Id.ToString());
        updatedUser.Should().NotBeNull();
        bool checkResult = await UserManager.CheckPasswordAsync(updatedUser!, "NewPassword123!");
        checkResult.Should().BeTrue();
    }
}
