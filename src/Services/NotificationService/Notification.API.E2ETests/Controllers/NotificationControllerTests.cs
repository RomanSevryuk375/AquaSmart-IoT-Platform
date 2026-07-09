using Microsoft.EntityFrameworkCore;
using Notification.Application.Features.Notifications.Queries.Shared;
using Notification.Domain.Entities;
using DomainNotification = Notification.Domain.Entities.Notification;

namespace Notification.API.E2ETests.Controllers;

public class NotificationControllerTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllNotificationsAsync_WithValidRequest_ReturnsOkAndMatchingNotifications()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        Ecosystem ecosystem = new EcosystemBuilder().WithUserId(user.Id).Build();
        DomainNotification notification = new NotificationBuilder()
            .WithUserId(user.Id)
            .WithEcosystemId(ecosystem.Id)
            .WithMessage("Water level is normal")
            .Build();

        DbContext.Users.Add(user);
        DbContext.Aquariums.Add(ecosystem);
        DbContext.Notifications.Add(notification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Notifications);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<NotificationDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<NotificationDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(notification.Id);
        content[0].Message.Should().Be("Water level is normal");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllNotificationsAsync_WithInvalidQuery_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Notifications}?level=999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllNotificationsAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Notifications);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetAllNotificationsAsync_TenantIsolation_ReturnsNotificationsOnlyForCurrentUser()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112255").Build();
        DomainNotification hackerNotification = new NotificationBuilder()
            .WithId(Guid.NewGuid())
            .WithUserId(hackerUserId)
            .WithEcosystemId(null)
            .WithMessage("Hacker alert")
            .Build();

        User ourUser = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        DomainNotification ourNotification = new NotificationBuilder()
            .WithUserId(ourUser.Id)
            .WithEcosystemId(null)
            .WithMessage("Our alert")
            .Build();

        DbContext.Users.AddRange(hackerUser, ourUser);
        DbContext.Notifications.AddRange(hackerNotification, ourNotification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.Routes.Notifications);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<NotificationDto>? content = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<NotificationDto>>();
        content.Should().NotBeNull();
        content.Should().ContainSingle();
        content![0].Id.Should().Be(ourNotification.Id);
        content.Should().NotContain(x => x.Id == hackerNotification.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetNotificationByIdAsync_WhenExists_ReturnsOkAndMatchingNotification()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        DomainNotification notification = new NotificationBuilder()
            .WithUserId(user.Id).WithEcosystemId(null).Build();

        DbContext.Users.Add(user);
        DbContext.Notifications.Add(notification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiConstants.Routes.Notifications}/{notification.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        NotificationDto? content = await response.Content.ReadFromJsonAsync<NotificationDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(notification.Id);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetNotificationByIdAsync_WithInvalidQueryOrBinding_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Notifications}/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetNotificationByIdAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Notifications}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetNotificationByIdAsync_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112244").Build();
        DomainNotification notification = new NotificationBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(null).Build();

        DbContext.Users.Add(hackerUser);
        DbContext.Notifications.Add(notification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Notifications}/{notification.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GetNotificationByIdAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.Routes.Notifications}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkNotificationAsReadAsync_WithValidRequest_Returns204NoContent()
    {
        // Arrange
        User user = new UserBuilder().WithId(NotificationTestConstants.UserId).Build();
        DomainNotification notification = new NotificationBuilder()
            .WithUserId(user.Id).WithEcosystemId(null).WithIsRead(false).Build();

        DbContext.Users.Add(user);
        DbContext.Notifications.Add(notification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.Notifications}/{notification.Id}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        DomainNotification? updatedNotification = await DbContext.Notifications
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == notification.Id);
        updatedNotification.Should().NotBeNull();
        updatedNotification!.IsRead.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkNotificationAsReadAsync_WithEmptyId_Returns400BadRequest()
    {
        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.Notifications}/{Guid.Empty}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkNotificationAsReadAsync_WhenDoesNotExist_Returns404NotFound()
    {
        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.Notifications}/{Guid.NewGuid()}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkNotificationAsReadAsync_WhenBelongsToAnotherUser_Returns404NotFound()
    {
        // Arrange
        var hackerUserId = Guid.NewGuid();
        User hackerUser = new UserBuilder()
            .WithId(hackerUserId).WithEmail("hacker@test.com").WithPhoneNumber("+375291112266").Build();
        DomainNotification notification = new NotificationBuilder()
            .WithId(Guid.NewGuid()).WithUserId(hackerUserId).WithEcosystemId(null).WithIsRead(false).Build();

        DbContext.Users.Add(hackerUser);
        DbContext.Notifications.Add(notification);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.Notifications}/{notification.Id}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task MarkNotificationAsReadAsync_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        HttpResponseMessage response = await Client.PutAsync(
            $"{ApiConstants.Routes.Notifications}/{Guid.NewGuid()}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
