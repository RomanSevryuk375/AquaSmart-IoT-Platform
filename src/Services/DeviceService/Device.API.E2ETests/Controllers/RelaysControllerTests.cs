using Contracts.Enums;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Application.Features.Relays.Command.UpdateRelay;
using Device.Application.Features.Relays.Query.Shared;
using Device.Domain.Entities;

namespace Device.API.E2ETests.Controllers;

public class RelaysControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRelaysAsync_WithValidRequest_ReturnsOk()
    {
        // Arrange
        Controller controller = new ControllerBuilder().WithUserId(TestConstants.UserId).Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Relays}?purpose={(int)relay.Purpose}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<RelayDto>? content = await response.Content.ReadFromJsonAsync<List<RelayDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(relay.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetRelayByIdAsync_WhenExists_ReturnsOk()
    {
        // Arrange
        Controller controller = new ControllerBuilder().WithUserId(TestConstants.UserId).Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Relays}/{relay.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RelayDto? content = await response.Content.ReadFromJsonAsync<RelayDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(relay.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddRelayAsync_WithValidCommand_ReturnsCreated()
    {
        // Arrange
        Controller controller = new ControllerBuilder().WithUserId(TestConstants.UserId).Build();

        DbContext.Controllers.Add(controller);
        await DbContext.SaveChangesAsync();

        var command = new AddRelayCommand
        {
            ControllerId = controller.Id,
            Name = "New Test Relay",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D5",
            IsNormallyOpen = true,
            Purpose = RelayPurpose.Light,
            IsActive = false,
            IsManual = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Relays, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.Relays);

        RelayCreatedResponse? content = await response.Content.ReadFromJsonAsync<RelayCreatedResponse>();

        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Name.Should().Be("New Test Relay");
        content.ControllerId.Should().Be(controller.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateRelayAsync_WithValidCommand_ReturnsNoContent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().WithUserId(TestConstants.UserId).Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        var command = new UpdateRelayCommand
        {
            ControllerId = controller.Id,
            ConnectionProtocol = ConnectionProtocol.Analog,
            ConnectionAddress = "A1",
            Purpose = RelayPurpose.Boiler,
            IsNormallyOpen = false
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Relays}/{relay.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteRelayAsync_WhenExists_ReturnsNoContent()
    {
        // Arrange
        Controller controller = new ControllerBuilder().WithUserId(TestConstants.UserId).Build();
        Relay relay = new RelayBuilder().WithControllerId(controller.Id).Build();

        DbContext.Controllers.Add(controller);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Relays}/{relay.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
