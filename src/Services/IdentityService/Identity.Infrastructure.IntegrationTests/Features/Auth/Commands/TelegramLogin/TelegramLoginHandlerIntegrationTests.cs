using BuildingBlocks.Domain.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.TelegramLogin;

namespace Identity.Infrastructure.IntegrationTests.Features.Auth.Commands.TelegramLogin;

public class TelegramLoginHandlerIntegrationTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnAccessToken_WhenTelegramChatIdIsLinked()
    {
        // Arrange
        const long chatId = 777666555L;
        User user = await CreateUserWithSubscriptionAsync("Linked User", "linked@example.com", "+375291110004");
        user.LinkTelegramChat(chatId);
        await DbContext.SaveChangesAsync();

        var command = new TelegramLoginCommand
        {
            ChatId = chatId
        };

        // Act
        Result<TelegramLoginResponseDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Value.ExpiresHours.Should().BeGreaterThan(0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldReturnNotFound_WhenTelegramChatIdIsNotLinked()
    {
        // Arrange
        var command = new TelegramLoginCommand
        {
            ChatId = 888999000L
        };

        // Act
        Result<TelegramLoginResponseDto> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
