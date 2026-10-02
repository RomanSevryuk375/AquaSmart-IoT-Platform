using DTOs;
using Handlers.Commands;
using Interfaces;

namespace TelegramBot.Application.UnitTests.Application.Handlers.Commands;

public class ShowRemindersHandlerTests
{
    private readonly INotificationApiClient _notificationClient = Substitute.For<INotificationApiClient>();
    private readonly ITelegramResponseService _response = Substitute.For<ITelegramResponseService>();
    private readonly ShowRemindersHandler _handler;

    public ShowRemindersHandlerTests()
    {
        _handler = new ShowRemindersHandler(_notificationClient, _response);
    }

    [Fact]
    public async Task HandleAsync_WhenNoReminders_SendsEmptyStateMessage()
    {
        // Arrange
        const long chatId = 300L;
        _notificationClient.GetAllRemindersAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ReminderDto>());

        // Act
        await _handler.HandleAsync(chatId, CancellationToken.None);

        // Assert
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _response.DidNotReceive().SendWithKeyboardAsync(
            Arg.Any<long>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRemindersExist_SendsWithInlineKeyboard()
    {
        // Arrange
        const long chatId = 301L;
        ReminderDto[] reminders = new[]
        {
            new ReminderDto
            {
                Id = Guid.NewGuid(),
                TaskName = "Подмена воды 30%",
                IntervalDays = 7,
                NextDueAt = DateTime.UtcNow.AddDays(3),
                EcosystemId = Guid.NewGuid()
            }
        };

        _notificationClient.GetAllRemindersAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(reminders);

        // Act
        await _handler.HandleAsync(chatId, CancellationToken.None);

        // Assert
        await _response.Received(1).SendWithKeyboardAsync(
            chatId, Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OverdueReminder_ContainsRedEmoji()
    {
        // Arrange
        const long chatId = 302L;
        ReminderDto[] reminders = new[]
        {
            new ReminderDto
            {
                Id = Guid.NewGuid(),
                TaskName = "Чистка фильтра",
                IntervalDays = 14,
                NextDueAt = DateTime.UtcNow.AddDays(-3),  // overdue
                EcosystemId = Guid.NewGuid()
            }
        };

        _notificationClient.GetAllRemindersAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(reminders);

        string? capturedText = null;
        await _response.SendWithKeyboardAsync(chatId,
            Arg.Do<string>(t => capturedText = t), Arg.Any<object>(), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(chatId, CancellationToken.None);

        // Assert
        capturedText.Should().Contain("🔴");
    }

    [Fact]
    public async Task HandleAsync_TodayReminder_ContainsYellowEmoji()
    {
        // Arrange
        const long chatId = 303L;
        ReminderDto[] reminders = new[]
        {
            new ReminderDto
            {
                Id = Guid.NewGuid(),
                TaskName = "Замена воды",
                IntervalDays = 7,
                NextDueAt = DateTime.UtcNow.Date,  // due today
                EcosystemId = Guid.NewGuid()
            }
        };

        _notificationClient.GetAllRemindersAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(reminders);

        string? capturedText = null;
        await _response.SendWithKeyboardAsync(chatId,
            Arg.Do<string>(t => capturedText = t), Arg.Any<object>(), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(chatId, CancellationToken.None);

        // Assert
        capturedText.Should().Contain("🟡");
    }
}
