using Microsoft.Extensions.Caching.Distributed;
using TelegramBotService.Infrastructure.Sessions;

namespace TelegramBot.Application.UnitTests.Infrastructure.Sessions;

public class RedisFsmSessionStoreTests
{
    private readonly IDistributedCache _cacheMock = Substitute.For<IDistributedCache>();
    private readonly RedisFsmSessionStore _store;

    public RedisFsmSessionStoreTests()
    {
        _store = new RedisFsmSessionStore(_cacheMock);
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ReturnsSessioWithNoneState()
    {
        // Arrange
        const long chatId = 123456L;
        _cacheMock.GetAsync($"fsm:{chatId}", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        // Act
        DialogueSessionData result = await _store.GetAsync(chatId);

        // Assert
        result.State.Should().Be(UserDialogueState.None);
    }

    [Fact]
    public async Task GetAsync_WhenDataExists_DeserializesCorrectly()
    {
        // Arrange
        const long chatId = 999L;
        var original = new DialogueSessionData
        {
            State = UserDialogueState.Log_WaitingForPh,
            SelectedEcosystemId = Guid.NewGuid(),
            SelectedEcosystemName = "Травник 150л"
        };

        // Save it first to test round-trip
        byte[]? captured = null;
        await _cacheMock.SetAsync(
            $"fsm:{chatId}",
            Arg.Do<byte[]>(b => captured = b),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());

        await _store.SaveAsync(chatId, original);

        if (captured is not null)
        {
            _cacheMock.GetAsync($"fsm:{chatId}", Arg.Any<CancellationToken>())
                .Returns(captured);
        }

        // Act
        DialogueSessionData result = await _store.GetAsync(chatId);

        // Assert
        result.State.Should().Be(UserDialogueState.Log_WaitingForPh);
        result.SelectedEcosystemName.Should().Be("Травник 150л");
    }

    [Fact]
    public async Task DeleteAsync_CallsRemoveWithCorrectKey()
    {
        // Arrange
        const long chatId = 777L;

        // Act
        await _store.DeleteAsync(chatId);

        // Assert
        await _cacheMock.Received(1).RemoveAsync($"fsm:{chatId}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_CallsSetWithCorrectKeyAndNonEmptyJson()
    {
        // Arrange
        const long chatId = 555L;
        var session = new DialogueSessionData
        {
            State = UserDialogueState.Reminder_WaitingForTaskName,
            SelectedEcosystemId = Guid.NewGuid()
        };

        // Act
        await _store.SaveAsync(chatId, session);

        // Assert
        await _cacheMock.Received(1).SetAsync(
            $"fsm:{chatId}",
            Arg.Is<byte[]>(b => b.Length > 0),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }
}
