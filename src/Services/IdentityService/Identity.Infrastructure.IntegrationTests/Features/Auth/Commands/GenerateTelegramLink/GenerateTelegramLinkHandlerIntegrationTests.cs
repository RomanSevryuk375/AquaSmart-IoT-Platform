using BuildingBlocks.Domain.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;
using ZiggyCreatures.Caching.Fusion;

namespace Identity.Infrastructure.IntegrationTests.Features.Auth.Commands.GenerateTelegramLink;

public class GenerateTelegramLinkHandlerIntegrationTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_ShouldGenerateTelegramLinkAndStoreTokenInCache_WhenUserExists()
    {
        // Arrange
        User user = await CreateUserWithSubscriptionAsync("Tg User", "tg.user@example.com", "+375291112233");
        IFusionCache cache = GetRequiredService<IFusionCache>();

        var command = new GenerateTelegramLinkCommand
        {
            UserId = user.Id
        };

        // Act
        Result<TelegramLinkTokenResponseDto> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Link.Should().StartWith("https://t.me/AquaSmartTestBot?start=");

        string token = result.Value.Link.Split("?start=")[1];
        token.Should().NotBeNullOrWhiteSpace();

        string? cachedUserId = await cache.GetOrDefaultAsync<string>($"tg-link:{token}");
        cachedUserId.Should().Be(user.Id.ToString());
    }
}
