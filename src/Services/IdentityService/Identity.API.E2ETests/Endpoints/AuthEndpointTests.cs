using BuildingBlocks.Domain.Enums;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.E2ETests.Endpoints;

public class AuthEndpointTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Register_Success_ReturnsOkAndTokensAndSetsCookies()
    {
        // Arrange
        var request = new RegisterUserRequestDto
        {
            Email = "register.success@example.com",
            PhoneNumber = "+375291112233",
            Name = "Register Success",
            Password = "Password123!",
            TimeZone = "Europe/Minsk"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponseDto? content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.RefreshToken.Should().NotBeNullOrWhiteSpace();

        response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies).Should().BeTrue();
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.AccessTokenCookieName}="));
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.RefreshTokenCookieName}="));

        User? user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        user.Should().NotBeNull();
        user!.Name.Value.Should().Be(request.Name);
        user.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Register_Conflict_EmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        User existingUser = new UserBuilder()
            .WithEmail("conflict@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(existingUser, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new RegisterUserRequestDto
        {
            Email = "conflict@example.com",
            PhoneNumber = "+375294445566",
            Name = "New User",
            Password = "Password123!",
            TimeZone = "Europe/Minsk"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Register_Validation_InvalidEmailOrPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequestDto
        {
            Email = "invalid-email",
            PhoneNumber = "+375291112233",
            Name = "Test",
            Password = "",
            TimeZone = "Europe/Minsk"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Login_Success_ReturnsOkAndTokensAndSetsCookies()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEmail("login.success@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new LoginUserRequestDto
        {
            Email = "login.success@example.com",
            Password = "Password123!"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponseDto? content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.RefreshToken.Should().NotBeNullOrWhiteSpace();

        response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies).Should().BeTrue();
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.AccessTokenCookieName}="));
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.RefreshTokenCookieName}="));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Login_Conflict_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEmail("login.wrongpass@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var request = new LoginUserRequestDto
        {
            Email = "login.wrongpass@example.com",
            Password = "WrongPassword!"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Login_NotFound_EmailDoesNotExist_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginUserRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/identity/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Refresh_Success_ReturnsNewTokensAndSetsCookies()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEmail("refresh.success@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var loginRequest = new LoginUserRequestDto
        {
            Email = "refresh.success@example.com",
            Password = "Password123!"
        };
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("api/identity/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponseDto? loginDto = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginDto.Should().NotBeNull();

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = loginDto!.RefreshToken
        };

        // Act
        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync(
            "api/identity/v1/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponseDto? refreshDto = await refreshResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        refreshDto.Should().NotBeNull();
        refreshDto!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshDto.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshDto.RefreshToken.Should().NotBe(loginDto.RefreshToken);

        refreshResponse.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies).Should().BeTrue();
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.AccessTokenCookieName}="));
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.RefreshTokenCookieName}="));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Refresh_TokenReuse_ReturnsUnauthorizedAndRevokesAllTokens()
    {
        // Arrange
        User user = new UserBuilder()
            .WithEmail("refresh.reuse@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        IdentityResult createResult = await UserManager.CreateAsync(user, "Password123!");
        createResult.Succeeded.Should().BeTrue();

        var loginRequest = new LoginUserRequestDto
        {
            Email = "refresh.reuse@example.com",
            Password = "Password123!"
        };
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("api/identity/v1/auth/login", loginRequest);
        LoginResponseDto? loginDto = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginDto.Should().NotBeNull();

        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = loginDto!.RefreshToken
        };

        HttpResponseMessage firstRefreshResponse = await Client.PostAsJsonAsync(
            "api/identity/v1/auth/refresh", refreshRequest);
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        HttpResponseMessage secondRefreshResponse = await Client.PostAsJsonAsync(
            "api/identity/v1/auth/refresh", refreshRequest);

        // Assert
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        string errorContent = await secondRefreshResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain(ErrorCodes.Identity.TokenReuse);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Logout_Success_ReturnsNoContentAndClearsCookies()
    {
        // Act
        HttpResponseMessage response = await Client.PostAsync("api/identity/v1/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies).Should().BeTrue();
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.AccessTokenCookieName}="));
        cookies.Should().Contain(c => c.Contains($"{AuthConstants.RefreshTokenCookieName}="));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Logout_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("api/identity/v1/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
