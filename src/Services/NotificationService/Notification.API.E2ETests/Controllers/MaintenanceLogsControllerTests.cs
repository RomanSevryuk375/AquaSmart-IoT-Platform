using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;
using Notification.Domain.Entities;

namespace Notification.API.E2ETests.Controllers;

public class MaintenanceLogsControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllLogsAsync_WithValidRequest_ReturnsOkAndMatchingLogs()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        MaintenanceLog log = new MaintenanceLogBuilder()
            .WithUserId(user.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithNotes("Daily cleanup notes")
            .Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.MaintenanceLogs.Add(log);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.MaintenanceLogs}?ecosystemId={ecosystem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<MaintenanceLogDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<MaintenanceLogDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(log.Id);
        content[0].Notes.Should().Be("Daily cleanup notes");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllLogsAsync_WithInvalidQuery_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.MaintenanceLogs}?ecosystemId=not-a-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllLogsAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.MaintenanceLogs);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllLogsAsync_TenantIsolation_ReturnsLogsOnlyForCurrentUser()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId)
            .WithEmail("hacker@test.com")
            .WithPhoneNumber("+375291112299")
            .Build();

        Ecosystem hackerEcosystem = new EcosystemBuilder().WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        MaintenanceLog hackerLog = new MaintenanceLogBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithEcosystemId(hackerEcosystem.Id)
            .WithNotes("Hacker log")
            .Build();

        // Seed our user and ecosystem
        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ourEcosystem = new EcosystemBuilder().WithUserId(ourUser.Id).Build();
        MaintenanceLog ourLog = new MaintenanceLogBuilder()
            .WithUserId(ourUser.Id)
            .WithEcosystemId(ourEcosystem.Id)
            .WithNotes("Our log")
            .Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.AddRange(hackerEcosystem, ourEcosystem);
        DbContext.MaintenanceLogs.AddRange(hackerLog, ourLog);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.MaintenanceLogs);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<MaintenanceLogDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<MaintenanceLogDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(ourLog.Id);
        content.Should().NotContain(x => x.Id == hackerLog.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetLogByIdAsync_WhenExists_ReturnsOkAndMatchingLog()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        MaintenanceLog log = new MaintenanceLogBuilder().WithUserId(user.Id).WithEcosystemId(ecosystem.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.MaintenanceLogs.Add(log);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.MaintenanceLogs}/{log.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        MaintenanceLogDto? content = await response.Content.ReadFromJsonAsync<MaintenanceLogDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(log.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetLogByIdAsync_WithInvalidQueryOrBinding_Returns400BadRequest()
    {
        // Act
        // Guid.Empty triggers GetMaintenanceLogByIdValidator validation failure
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.MaintenanceLogs}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetLogByIdAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.MaintenanceLogs}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetLogByIdAsync_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId)
            .WithEmail("hacker@test.com")
            .WithPhoneNumber("+375291112277")
            .Build();

        Ecosystem hackerEcosystem = new EcosystemBuilder().WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();
        MaintenanceLog log = new MaintenanceLogBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithEcosystemId(hackerEcosystem.Id)
            .Build();

        DbContext.Users.Add(hackerUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        DbContext.MaintenanceLogs.Add(log);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.MaintenanceLogs}/{log.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetLogByIdAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.MaintenanceLogs}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddLogAsync_WithValidCommand_Returns201CreatedAndLocationHeader()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateMaintenanceLogCommand
        {
            EcosystemId = ecosystem.Id,
            ActionDate = DateTime.UtcNow,
            Notes = "Added new filter pad",
            Metrics = new Dictionary<string, double> { { "pH", 7.2 } }
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.MaintenanceLogs, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(ApiConstants.Routes.MaintenanceLogs);

        Guid createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        MaintenanceLog? logInDb = await DbContext.MaintenanceLogs
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == createdId);
        logInDb.Should().NotBeNull();
        logInDb!.Notes.Should().Be("Added new filter pad");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddLogAsync_WithInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var command = new CreateMaintenanceLogCommand
        {
            EcosystemId = Guid.Empty,
            ActionDate = DateTime.UtcNow.AddDays(2),
            Notes = new string('A', 2000),
            Metrics = null!
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.MaintenanceLogs, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddLogAsync_WhenEcosystemBelongsToAnotherUser_Returns409Conflict()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId)
            .WithEmail("hacker@test.com")
            .WithPhoneNumber("+375291112288")
            .Build();
        Ecosystem hackerEcosystem = new EcosystemBuilder().WithId(Guid.NewGuid()).WithUserId(hackerUserId).Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Aquariums.Add(hackerEcosystem);
        await DbContext.SaveChangesAsync();

        var command = new CreateMaintenanceLogCommand
        {
            EcosystemId = hackerEcosystem.Id,
            ActionDate = DateTime.UtcNow,
            Notes = "Hacking try"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.MaintenanceLogs, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task AddLogAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var command = new CreateMaintenanceLogCommand
        {
            EcosystemId = Guid.NewGuid(),
            ActionDate = DateTime.UtcNow
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.Routes.MaintenanceLogs, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
