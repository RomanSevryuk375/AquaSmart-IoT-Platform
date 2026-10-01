using DTOs;
using Handlers.Fsm;
using Interfaces;

namespace TelegramBot.Application.UnitTests.Application.Handlers.Fsm;

public class AddLogFsmHandlerTests
{
    private readonly IFsmSessionStore _sessionStore = Substitute.For<IFsmSessionStore>();
    private readonly INotificationApiClient _notificationClient = Substitute.For<INotificationApiClient>();
    private readonly IControlApiClient _controlClient = Substitute.For<IControlApiClient>();
    private readonly ITelegramResponseService _response = Substitute.For<ITelegramResponseService>();

    private readonly AddLogFsmHandler _handler;

    public AddLogFsmHandlerTests()
    {
        _handler = new AddLogFsmHandler(_sessionStore, _notificationClient, _controlClient, _response);
    }

    // ── StartAsync ────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_WhenNoEcosystems_SendsNoEcosystemMessage()
    {
        // Arrange
        const long chatId = 100L;
        _controlClient.GetUserEcosystemsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<EcosystemDto>());

        // Act
        await _handler.StartAsync(chatId, CancellationToken.None);

        // Assert
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _sessionStore.DidNotReceive().SaveAsync(Arg.Any<long>(), Arg.Any<DialogueSessionData>(),
            Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenEcosystemsExist_SetsWaitingForEcosystemState()
    {
        // Arrange
        const long chatId = 101L;
        EcosystemDto[] ecosystems = new[] { new EcosystemDto { Id = Guid.NewGuid(), Name = "Травник" } };
        _controlClient.GetUserEcosystemsAsync(chatId, Arg.Any<CancellationToken>())
            .Returns(ecosystems);

        // Act
        await _handler.StartAsync(chatId, CancellationToken.None);

        // Assert
        await _sessionStore.Received(1).SaveAsync(
            chatId,
            Arg.Is<DialogueSessionData>(s => s.State == UserDialogueState.Log_WaitingForEcosystem),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        await _response.Received(1).SendWithKeyboardAsync(
            chatId, Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    // ── pH step ───────────────────────────────────────────────────

    [Fact]
    public async Task HandleInputAsync_WaitingForPh_WithValidPh_AdvancesToKhState()
    {
        // Arrange
        const long chatId = 102L;
        var session = new DialogueSessionData
        {
            State = UserDialogueState.Log_WaitingForPh,
            SelectedEcosystemId = Guid.NewGuid(),
            SelectedEcosystemName = "Травник"
        };

        // Act
        await _handler.HandleInputAsync(chatId, "7.2", session, CancellationToken.None);

        // Assert
        await _sessionStore.Received(1).SaveAsync(
            chatId,
            Arg.Is<DialogueSessionData>(s =>
                s.State == UserDialogueState.Log_WaitingForKh && s.Ph == 7.2),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleInputAsync_WaitingForPh_WithInvalidPh_SendsErrorAndDoesNotAdvance()
    {
        // Arrange
        const long chatId = 103L;
        var session = new DialogueSessionData { State = UserDialogueState.Log_WaitingForPh };

        // Act
        await _handler.HandleInputAsync(chatId, "not-a-number", session, CancellationToken.None);

        // Assert
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _sessionStore.DidNotReceive().SaveAsync(Arg.Any<long>(), Arg.Any<DialogueSessionData>(),
            Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("15.5")]  // pH > 14 is invalid
    public async Task HandleInputAsync_WaitingForPh_OutOfRange_SendsError(string invalidPh)
    {
        // Arrange
        const long chatId = 104L;
        var session = new DialogueSessionData { State = UserDialogueState.Log_WaitingForPh };

        // Act
        await _handler.HandleInputAsync(chatId, invalidPh, session, CancellationToken.None);

        // Assert
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _sessionStore.DidNotReceive().SaveAsync(Arg.Any<long>(), Arg.Any<DialogueSessionData>(),
            Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    // ── Cancel ────────────────────────────────────────────────────

    [Fact]
    public async Task HandleInputAsync_WhenCancelInput_DeletesSessionAndSendsConfirmation()
    {
        // Arrange
        const long chatId = 105L;
        var session = new DialogueSessionData { State = UserDialogueState.Log_WaitingForKh };

        // Act
        await _handler.HandleInputAsync(chatId, "Отмена", session, CancellationToken.None);

        // Assert
        await _sessionStore.Received(1).DeleteAsync(chatId, Arg.Any<CancellationToken>());
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Notes step → submit ───────────────────────────────────────

    [Fact]
    public async Task HandleInputAsync_WaitingForNotes_SubmitsLogAndClearsSession()
    {
        // Arrange
        const long chatId = 106L;
        var ecosystemId = Guid.NewGuid();
        var session = new DialogueSessionData
        {
            State = UserDialogueState.Log_WaitingForNotes,
            SelectedEcosystemId = ecosystemId,
            SelectedEcosystemName = "Травник",
            Ph = 7.0,
            Kh = 8.0,
            No3 = 15.0
        };

        _notificationClient.CreateMaintenanceLogAsync(chatId, Arg.Any<CreateMaintenanceLogRequest>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        // Act
        await _handler.HandleInputAsync(chatId, "Заменил синтепон в фильтре", session, CancellationToken.None);

        // Assert
        await _notificationClient.Received(1).CreateMaintenanceLogAsync(
            chatId,
            Arg.Is<CreateMaintenanceLogRequest>(r =>
                r.EcosystemId == ecosystemId &&
                r.Metrics.ContainsKey("pH") &&
                r.Notes == "Заменил синтепон в фильтре"),
            Arg.Any<CancellationToken>());

        await _sessionStore.Received(1).DeleteAsync(chatId, Arg.Any<CancellationToken>());
        await _response.Received(1).SendTextAsync(chatId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
