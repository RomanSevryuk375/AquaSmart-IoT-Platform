using Device.Application.Extesions;
using Device.Application.Features.RelayCommands.Query.GetPending;
using Device.Domain.Entities;

namespace Device.API.E2ETests.Controllers;

public class RelayCommandsControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetPendingCommands_WithValidHeaders_Returns200Ok()
    {
        // Arrange
        var hasher = new MyHasher();

        Controller controller = new ControllerBuilder()
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

        Client.DefaultRequestHeaders.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Commands}/pending/{controller.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<RelayCommandDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<RelayCommandDto>>();
        content.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsCompleted_WithValidHeaders_Returns204NoContent()
    {
        // Arrange
        var hasher = new MyHasher();

        Controller controller = new ControllerBuilder()
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

        Client.DefaultRequestHeaders.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.PostAsync(
            $"{ApiConstants.Routes.Commands}/{command.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkAsFailed_WithValidHeadersAndBody_Returns204NoContent()
    {
        // Arrange
        var hasher = new MyHasher();

        Controller controller = new ControllerBuilder()
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

        Client.DefaultRequestHeaders.Add(ApiConstants.Headers.DeviceToken, TestConstants.ValidRawToken);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiConstants.Routes.Commands}/{command.Id}/fail", "Hardware fault");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
