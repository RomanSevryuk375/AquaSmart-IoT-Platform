using BuildingBlocks.Presentation.Constants;
using Device.Application.Extesions;
using Device.Application.Features.RelayCommands.Query.GetPending;
using Device.Domain.Entities;

namespace Device.API.E2ETests.Endpoints;

public class RelayCommandsEndpointTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPendingCommands_WithValidHeaders_Returns200Ok()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B1")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{ApiConstants.Routes.Commands}/pending/{controller.Id}");
        request.Headers.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<RelayCommandDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<RelayCommandDto>>();
        content.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPendingCommands_WithoutDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B2")
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{ApiConstants.Routes.Commands}/pending/{controller.Id}");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPendingCommands_WithInvalidDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B3")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{ApiConstants.Routes.Commands}/pending/{controller.Id}");
        request.Headers.Add(ApiConstants.Headers.DeviceToken, "invalid_token");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsCompleted_WithValidHeaders_Returns204NoContent()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B4")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/complete");
        request.Headers.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsCompleted_WithoutDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B5")
            .Build();

        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/complete");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsCompleted_WithInvalidDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B6")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/complete");
        request.Headers.Add(ApiConstants.Headers.DeviceToken, "invalid_token");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsFailed_WithValidHeadersAndBody_Returns204NoContent()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B7")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/fail")
        {
            Content = JsonContent.Create("Hardware fault")
        };
        request.Headers.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsFailed_WithoutDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B8")
            .Build();

        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/fail")
        {
            Content = JsonContent.Create("Hardware fault")
        };

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsFailed_WithInvalidDeviceToken_Returns401Unauthorized()
    {
        // Arrange
        var hasher = new MyHasher();
        var controllerId = Guid.NewGuid();

        Controller controller = new ControllerBuilder()
            .WithId(controllerId)
            .WithMacAddress("00:11:22:33:44:B9")
            .WithDeviceTokenHash(hasher.Generate(TestConstants.ValidRawToken))
            .Build();

        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();
        RelayCommand command = new RelayCommandBuilder()
            .WithControllerId(controller.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        DbContext.RelayCommands.Add(command);
        await DbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiConstants.Routes.Commands}/{command.Id}/fail")
        {
            Content = JsonContent.Create("Hardware fault")
        };
        request.Headers.Add(ApiConstants.Headers.DeviceToken, "invalid_token");

        // Act
        HttpResponseMessage response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ToggleRelayState_WithValidAuth_Returns200Ok()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .AsActive(false)
            .AsAuto()
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PostAsync(
            $"{ApiConstants.Routes.Commands}/toggle-state/{relay.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bool content = await response.Content.ReadFromJsonAsync<bool>();
        content.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task ToggleRelayMode_WithValidAuth_Returns200Ok()
    {
        // Arrange
        Controller controller = new ControllerBuilder().Build();

        Relay relay = new RelayBuilder()
            .WithControllerId(controller.Id)
            .Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PostAsync(
            $"{ApiConstants.Routes.Commands}/toggle-mode/{relay.Id}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bool content = await response.Content.ReadFromJsonAsync<bool>();
        content.Should().BeFalse();
    }
}
