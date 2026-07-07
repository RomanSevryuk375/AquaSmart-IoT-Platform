using Control.API.Controllers;
using Control.Application.Features.Schedules.Commands.CreateSchedule;
using Control.Application.Features.Schedules.Commands.UpdateSchedule;
using Control.Application.Features.Schedules.Queries.Shared;
using Control.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Control.API.E2ETests.Controllers;

public class SchedulesControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            ApiConstants.Routes.Schedules);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_WithValidRequest_Returns200OKAndCorrectData()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<ScheduleDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<ScheduleDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle(s => s.Id == schedule.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAll_TenantIsolation_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenExists_Returns200OK()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ScheduleDto? content = await response.Content
            .ReadFromJsonAsync<ScheduleDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(schedule.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetById_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new CreateScheduleCommand
        {
            EcosystemId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            CronExpression = "0 12 * * *",
            DurationMin = 30.0,
            IsFadeMode = false,
            IsEnabled = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Schedules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithValidData_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        var command = new CreateScheduleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = relay.Id,
            CronExpression = "0 12 * * *",
            DurationMin = 30.0,
            IsFadeMode = false,
            IsEnabled = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Schedules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.Schedules);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        Schedule? dbSchedule = await DbContext.Schedules
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == createdId);
        dbSchedule.Should().NotBeNull();
        dbSchedule!.CronExpression.Value.Should().Be(command.CronExpression);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_WithInvalidCronExpression_Returns400BadRequest()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        var command = new CreateScheduleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = relay.Id,
            CronExpression = "invalid_cron",
            DurationMin = 30.0,
            IsFadeMode = false,
            IsEnabled = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Schedules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Create_ForAnotherUserEcosystem_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        await DbContext.SaveChangesAsync();

        var command = new CreateScheduleCommand
        {
            EcosystemId = ecosystem.Id,
            RelayId = relay.Id,
            CronExpression = "0 12 * * *",
            DurationMin = 30.0,
            IsFadeMode = false,
            IsEnabled = true
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            ApiConstants.Routes.Schedules, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new UpdateScheduleCommand
        {
            CronExpression = "0 18 * * *",
            DurationMin = 45.0,
            IsFadeMode = true,
            IsEnabled = false
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WithValidData_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        var command = new UpdateScheduleCommand
        {
            CronExpression = "0 18 * * *",
            DurationMin = 45.0,
            IsFadeMode = true,
            IsEnabled = false
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Schedule? dbSchedule = await DbContext.Schedules
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == schedule.Id);
        dbSchedule.Should().NotBeNull();
        dbSchedule!.CronExpression.Value.Should().Be(command.CronExpression);
        dbSchedule.DurationMin.Should().Be(command.DurationMin);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var command = new UpdateScheduleCommand
        {
            CronExpression = "0 18 * * *",
            DurationMin = 45.0,
            IsFadeMode = true,
            IsEnabled = false
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Update_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        var command = new UpdateScheduleCommand
        {
            CronExpression = "0 18 * * *",
            DurationMin = 45.0,
            IsFadeMode = true,
            IsEnabled = false
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task SetIsActive_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var request = new SetIsActiveScheduleRequest { IsActive = false };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}/active", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task SetIsActive_WithValidData_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .WithIsEnabled(true)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        var request = new SetIsActiveScheduleRequest { IsActive = false };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}/active", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Schedule? dbSchedule = await DbContext.Schedules
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == schedule.Id);
        dbSchedule!.IsEnabled.Should().BeFalse();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task SetIsActive_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var request = new SetIsActiveScheduleRequest { IsActive = false };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}/active", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task SetIsActive_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        var request = new SetIsActiveScheduleRequest { IsActive = false };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}/active", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenExists_Returns204NoContent()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(ControlTestConstants.UserId)
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Schedule? dbSchedule = await DbContext.Schedules
            .AsNoTracking().FirstOrDefaultAsync(s => s.Id == schedule.Id);
        dbSchedule.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Schedules}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Delete_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        Ecosystem ecosystem = new EcosystemBuilder()
            .WithUserId(Guid.NewGuid())
            .Build();

        Relay relay = new RelayBuilder()
            .WithEcosystemId(ecosystem.Id)
            .Build();

        Schedule schedule = new ScheduleBuilder()
            .WithEcosystemId(ecosystem.Id)
            .WithRelayId(relay.Id)
            .Build();

        DbContext.Aquariums.Add(ecosystem);
        DbContext.Relays.Add(relay);
        DbContext.Schedules.Add(schedule);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync(
            $"{ApiConstants.Routes.Schedules}/{schedule.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
