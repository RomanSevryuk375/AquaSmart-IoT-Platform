using Contracts.Results;
using FluentAssertions;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessReminders;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.TestShared.Builders;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessReminders;

public class ProcessRemindersHandlerTests
{
    private readonly IReminderRepository _reminderRepoMock = Substitute.For<IReminderRepository>();
    private readonly INotificationRepository _notificationRepoMock = Substitute.For<INotificationRepository>();
    private readonly IUserRepository _userRepoMock = Substitute.For<IUserRepository>();
    private readonly ProcessRemindersHandler _handler;

    public ProcessRemindersHandlerTests()
    {
        _handler = new ProcessRemindersHandler(
            _reminderRepoMock,
            _notificationRepoMock,
            _userRepoMock);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoPendingReminders_ReturnsSuccess()
    {
        // Arrange
        _reminderRepoMock.GetPendingRemindersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Reminder>)null!);

        var command = new ProcessRemindersCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userRepoMock.DidNotReceive().GetAllUsersByIdAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenUserNotFound_SkipsReminder()
    {
        // Arrange
        Reminder reminder = new ReminderBuilder().Build();
        _reminderRepoMock.GetPendingRemindersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Reminder> { reminder });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var command = new ProcessRemindersCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.LastNotifiedAt.Should().BeNull();
        await _notificationRepoMock.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenLastNotifiedIsTodayLocal_SkipsReminder()
    {
        // Arrange
        User user = new UserBuilder()
            .WithTimeZone("UTC")
            .Build();

        Reminder reminder = new ReminderBuilder()
            .WithUserId(user.Id)
            .Build();
        reminder.MarkAsNotified();

        _reminderRepoMock.GetPendingRemindersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Reminder> { reminder });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessRemindersCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _notificationRepoMock.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNotInDaytimeLocal_SkipsReminder()
    {
        // Arrange
        TimeZoneInfo? nightTimeTz = TimeZoneInfo.GetSystemTimeZones()
            .FirstOrDefault(tz =>
            {
                int localHour = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Hour;
                return localHour < 9 || localHour > 21;
            });

        if (nightTimeTz == null)
        {
            return;
        }

        User user = new UserBuilder()
            .WithTimeZone(nightTimeTz.Id)
            .Build();

        Reminder reminder = new ReminderBuilder()
            .WithUserId(user.Id)
            .Build();

        _reminderRepoMock.GetPendingRemindersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Reminder> { reminder });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessRemindersCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.LastNotifiedAt.Should().BeNull();
        await _notificationRepoMock.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenInDaytimeLocal_NotifiesUserAndMarksAsNotified()
    {
        // Arrange
        TimeZoneInfo? daytimeTz = TimeZoneInfo.GetSystemTimeZones()
            .FirstOrDefault(tz =>
            {
                int localHour = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Hour;
                return localHour >= 9 && localHour <= 21;
            });

        if (daytimeTz == null)
        {
            return;
        }

        User user = new UserBuilder()
            .WithTimeZone(daytimeTz.Id)
            .Build();

        Reminder reminder = new ReminderBuilder()
            .WithUserId(user.Id)
            .Build();

        _reminderRepoMock.GetPendingRemindersAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Reminder> { reminder });

        _userRepoMock.GetAllUsersByIdAsync(Arg.Is<List<Guid>>(x => x.Contains(user.Id)), Arg.Any<CancellationToken>())
            .Returns(new List<User> { user });

        var command = new ProcessRemindersCommand();

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reminder.LastNotifiedAt.Should().NotBeNull();
        reminder.LastNotifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        await _notificationRepoMock.Received(1).AddAsync(
            Arg.Is<Domain.Entities.Notification>(n => n.UserId == user.Id && n.EcosystemId == reminder.EcosystemId),
            Arg.Any<CancellationToken>());
    }
}
