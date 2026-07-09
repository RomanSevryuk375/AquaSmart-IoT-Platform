using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.Reminders.Commands.CreateReminder;
using Notification.Application.Features.Reminders.Commands.UpdateReminder;
using Notification.Application.Features.Reminders.Queries.Shared;
using Notification.Domain.Entities;

namespace Notification.API.E2ETests.Controllers;

public class RemindersControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRemindersAsync_WithValidRequest_ReturnsOkAndMatchingReminders()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        Reminder reminder = new ReminderBuilder()
            .WithUserId(user.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithTaskName("Clean skimmer cup")
            .Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Reminders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<ReminderDto>? content = await response.Content.ReadFromJsonAsync<IReadOnlyList<ReminderDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(reminder.Id);
        content[0].TaskName.Should().Be("Clean skimmer cup");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRemindersAsync_WithInvalidQuery_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}?skip=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRemindersAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Reminders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllRemindersAsync_TenantIsolation_ReturnsRemindersOnlyForCurrentUser()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112291").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        Reminder hackerReminder = new ReminderBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithEcosystemId(hackerEcosystem.Id)
            .WithTaskName("Hacker task")
            .Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ourEcosystem = new EcosystemBuilder().WithUserId(ourUser.Id).Build();
        Reminder ourReminder = new ReminderBuilder()
            .WithUserId(ourUser.Id)
            .WithEcosystemId(ourEcosystem.Id)
            .WithTaskName("Our task")
            .Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.AddRange(hackerEcosystem, ourEcosystem);
        DbContext.Reminders.AddRange(hackerReminder, ourReminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Reminders);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<ReminderDto>? content = await response.Content.ReadFromJsonAsync<IReadOnlyList<ReminderDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(ourReminder.Id);
        content.Should().NotContain(x => x.Id == hackerReminder.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetReminderByIdAsync_WhenExists_ReturnsOkAndMatchingReminder()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        Reminder reminder = new ReminderBuilder().WithUserId(user.Id).WithEcosystemId(ecosystem.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}/{reminder.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ReminderDto? content = await response.Content.ReadFromJsonAsync<ReminderDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(reminder.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetReminderByIdAsync_WithInvalidQueryOrBinding_Returns400BadRequest()
    {
        // Act
        // Guid.Empty triggers GetReminderByIdValidator validation failure
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetReminderByIdAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetReminderByIdAsync_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112292").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        Reminder reminder = new ReminderBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(hackerEcosystem.Id).Build();

        DbContext.Users.Add(hackerUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}/{reminder.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetReminderByIdAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CreateReminderAsync_WithValidCommand_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateReminderCommand
        {
            EcosystemId = ecosystem.Id,
            TaskName = "Water change",
            IntervalDays = 7
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Reminders, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.Reminders);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        Reminder? reminderInDb = await DbContext.Reminders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == createdId);
        reminderInDb.Should().NotBeNull();
        reminderInDb!.TaskName.Value.Should().Be("Water change");
        reminderInDb.IntervalDays.Should().Be(7);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CreateReminderAsync_WithInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var command = new CreateReminderCommand
        {
            EcosystemId = Guid.Empty, // Invalid empty Guid
            TaskName = "", // Invalid empty task name
            IntervalDays = -1 // Invalid negative interval
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Reminders, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CreateReminderAsync_WhenEcosystemBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112293").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder().WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateReminderCommand
        {
            EcosystemId = hackerEcosystem.Id,
            TaskName = "Hacked water change",
            IntervalDays = 3
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Reminders, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CreateReminderAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new CreateReminderCommand
        {
            EcosystemId = Guid.NewGuid(),
            TaskName = "Water change",
            IntervalDays = 7
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.Reminders, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateReminderAsync_WithValidCommand_Returns204NoContent()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        Reminder reminder = new ReminderBuilder().WithUserId(user.Id).WithEcosystemId(ecosystem.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        var command = new UpdateReminderCommand
        {
            TaskName = "New Feed Fish Task",
            IntervalDays = 14
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Reminders}/{reminder.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Reminder? updatedReminder = await DbContext.Reminders
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == reminder.Id);
        updatedReminder.Should().NotBeNull();
        updatedReminder!.TaskName.Value.Should().Be("New Feed Fish Task");
        updatedReminder.IntervalDays.Should().Be(14);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateReminderAsync_WithInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var command = new UpdateReminderCommand
        {
            TaskName = "", // Invalid empty name
            IntervalDays = 0 // Invalid interval
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateReminderAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var command = new UpdateReminderCommand
        {
            TaskName = "Valid Task Name",
            IntervalDays = 7
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateReminderAsync_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112294").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        Reminder reminder = new ReminderBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(hackerEcosystem.Id).Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        var command = new UpdateReminderCommand
        {
            TaskName = "Hacked Update",
            IntervalDays = 10
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Reminders}/{reminder.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task UpdateReminderAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new UpdateReminderCommand
        {
            TaskName = "Valid Task Name",
            IntervalDays = 7
        };

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CompleteReminderAsync_WithValidRequest_Returns204NoContent()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        Reminder reminder = new ReminderBuilder()
            .WithUserId(user.Id).WithEcosystemId(ecosystem.Id).WithIsCompleted(false).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiConstants.Routes.Reminders}/{reminder.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Reminder? updatedReminder = await DbContext.Reminders
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == reminder.Id);
        updatedReminder.Should().NotBeNull();
        updatedReminder!.LastDoneAt.Should().NotBeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CompleteReminderAsync_WithInvalidQuery_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.Empty}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CompleteReminderAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CompleteReminderAsync_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112295").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        Reminder reminder = new ReminderBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(hackerEcosystem.Id).Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiConstants.Routes.Reminders}/{reminder.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task CompleteReminderAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.PatchAsync(
            $"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteReminderAsync_WithValidRequest_Returns204NoContent()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        Reminder reminder = new ReminderBuilder().WithUserId(user.Id).WithEcosystemId(ecosystem.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.Routes.Reminders}/{reminder.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Reminder? deletedReminder = await DbContext.Reminders
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == reminder.Id);
        deletedReminder.Should().BeNull();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteReminderAsync_WithInvalidQuery_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.Routes.Reminders}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteReminderAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteReminderAsync_WhenBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112296").Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        Reminder reminder = new ReminderBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(hackerEcosystem.Id).Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        DbContext.Reminders.Add(reminder);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.Routes.Reminders}/{reminder.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task DeleteReminderAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.Routes.Reminders}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
