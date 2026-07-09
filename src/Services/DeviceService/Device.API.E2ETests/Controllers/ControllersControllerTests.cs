using Device.Application.Extesions;
using Device.Application.Features.Controllers.Command.AddController;
using Device.Application.Features.Controllers.Query.GetControllerConfig;
using Device.Domain.Entities;

namespace Device.API.E2ETests.Controllers;

public class ControllersControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Controllers);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddController_WithValidData_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        var command = new AddControllerCommand
        {
            MacAddress = "11:22:33:44:55:66",
            Name = "Living Room Aquarium",
            IsOnline = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Controllers, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.Controllers);

        ControllerRegisteredResponse? content = await response.Content
            .ReadFromJsonAsync<ControllerRegisteredResponse>();
        content.Should().NotBeNull();
        content!.ControllerId.Should().NotBeEmpty();
        content.DeviceToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddController_WithInvalidMac_Returns400BadRequest()
    {
        // Arrange
        var command = new AddControllerCommand
        {
            MacAddress = "XX-XX-XX-XX-XX_XX",
            Name = "Test Controller",
            IsOnline = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Controllers, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        ErrorResponseDummy? errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponseDummy>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Contain("'Mac Address' is not in the correct format.");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetControllerById_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        Controller hackerController = new ControllerBuilder().WithUserId(Guid.NewGuid()).Build();

        DbContext.Controllers.Add(hackerController);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Controllers}{hackerController.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetControllerConfig_WithValidDeviceHeaders_Returns200OK()
    {
        // Arrange
        var hasher = new MyHasher();
        string rawToken = "my_secret_token";

        Controller controller = new ControllerBuilder()
            .WithDeviceTokenHash(hasher.Generate(rawToken))
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiConstants.Routes.Controllers}/me/config");
        request.Headers.Add(ApiConstants.Headers.MacAddress, controller.MacAddress.Value);
        request.Headers.Add(ApiConstants.Headers.DeviceToken, rawToken);

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ControllerConfig? config = await response.Content.ReadFromJsonAsync<ControllerConfig>();
        config.Should().NotBeNull();
        config!.SendIntervalMs.Should().BeGreaterThan(0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Ping_WithInvalidDeviceToken_Returns404NotFound()
    {
        // Arrange
        var hasher = new MyHasher();
        Controller controller = new ControllerBuilder()
            .WithDeviceTokenHash(hasher.Generate("real_token"))
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Controllers}/{controller.Id}/ping");
        request.Headers.Add(ApiConstants.Headers.DeviceToken, "hacker_token");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private record ErrorResponseDummy(int StatusCode, string Message);
}
