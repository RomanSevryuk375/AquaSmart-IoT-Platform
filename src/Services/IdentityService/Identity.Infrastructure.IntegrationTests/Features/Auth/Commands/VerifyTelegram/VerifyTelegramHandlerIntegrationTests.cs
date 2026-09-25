using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data.Outbox;
using IdentityService.Application.Features.Auth.Commands.VerifyTelegram;
using ZiggyCreatures.Caching.Fusion;

namespace Identity.Infrastructure.IntegrationTests.Features.Auth.Commands.VerifyTelegram;

public class VerifyTelegramHandlerIntegrationTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldLinkTelegramChatAndCreateOutboxMessage_WhenTokenIsValid()
    {
        // Arrange
        User user = await CreateUserWithSubscriptionAsync("Alice", "alice@example.com", "+375291110001");
        IFusionCache cache = GetRequiredService<IFusionCache>();

        string token = Guid.NewGuid().ToString("N");
        await cache.SetAsync($"tg-link:{token}", user.Id.ToString(), TimeSpan.FromMinutes(10));

        const long chatId = 123456789L;
        var command = new VerifyTelegramCommand
        {
            Token = token,
            ChatId = chatId
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        User? updatedUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.TelegramChatId.Should().Be(chatId);

        // Cache token should be removed
        string? cachedUserId = await cache.GetOrDefaultAsync<string>($"tg-link:{token}");
        cachedUserId.Should().BeNull();

        // Outbox message should be produced
        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().ContainSingle(m => m.Type.Contains("TelegramAccountLinkedDomainEvent"));
        OutboxMessage outbox = outboxMessages.Single(m => m.Type.Contains("TelegramAccountLinkedDomainEvent"));
        outbox.Content.Should().Contain(user.Id.ToString());
        outbox.Content.Should().Contain(chatId.ToString());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnNotFound_WhenTokenIsExpiredOrNotFound()
    {
        // Arrange
        var command = new VerifyTelegramCommand
        {
            Token = "unknown-token-12345",
            ChatId = 987654321L
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("TelegramLink.InvalidOrExpired");

        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages
            .AsNoTracking()
            .ToListAsync();
        outboxMessages.Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnConflict_WhenTelegramChatIdAlreadyLinkedToAnotherUser()
    {
        // Arrange
        const long existingChatId = 999888777L;
        User user1 = await CreateUserWithSubscriptionAsync("User One", "user1@example.com", "+375291110002");
        user1.LinkTelegramChat(existingChatId);
        await DbContext.SaveChangesAsync();

        User user2 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName("User Two")
            .WithEmail("user2@example.com")
            .WithPhoneNumber("+375291110003")
            .WithSubscriptionId(user1.SubscriptionId)
            .Build();
        await DbContext.Users.AddAsync(user2);
        await DbContext.SaveChangesAsync();
        IFusionCache cache = GetRequiredService<IFusionCache>();

        string token = Guid.NewGuid().ToString("N");
        await cache.SetAsync($"tg-link:{token}", user2.Id.ToString(), TimeSpan.FromMinutes(10));

        var command = new VerifyTelegramCommand
        {
            Token = token,
            ChatId = existingChatId
        };

        // Act
        Result result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Telegram.AlreadyLinked");

        User? checkUser2 = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user2.Id);

        checkUser2.Should().NotBeNull();
        checkUser2!.TelegramChatId.Should().BeNull();
    }
}
