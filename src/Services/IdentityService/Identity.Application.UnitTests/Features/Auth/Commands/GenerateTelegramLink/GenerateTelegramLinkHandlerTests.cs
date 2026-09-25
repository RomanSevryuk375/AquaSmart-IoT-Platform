using BuildingBlocks.Domain.Results;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Auth.Commands.GenerateTelegramLink;
using IdentityService.Application.Options;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Identity.Application.UnitTests.Features.Auth.Commands.GenerateTelegramLink;

public class GenerateTelegramLinkHandlerTests
{
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();
    private readonly GenerateTelegramLinkHandler _handler;

    private const string BotName = "@AquaTest_bot";
    private const string BotNameWithoutAt = "AquaTest_bot";

    public GenerateTelegramLinkHandlerTests()
    {
        var options = Options.Create(new TelegramBotOptions
        {
            Name = BotName,
            BotSecretKey = "test-secret"
        });

        _handler = new GenerateTelegramLinkHandler(_cacheMock, options);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_Always_StoresUserIdInCacheViaCacheSetAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new GenerateTelegramLinkCommand { UserId = userId };

        // Act
        Result<TelegramLinkTokenResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // FusionCache.SetAsync with duration is an extension method — verify via overload with FusionCacheEntryOptions
        await _cacheMock.Received(1).SetAsync(
            Arg.Is<string>(key => key.StartsWith("tg-link:")),
            userId.ToString(),
            Arg.Is<FusionCacheEntryOptions>(opts => opts.Duration == TimeSpan.FromMinutes(10)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_Always_ReturnsLinkWithBotUsernameWithoutAtPrefix()
    {
        // Arrange
        var command = new GenerateTelegramLinkCommand { UserId = Guid.NewGuid() };

        // Act
        Result<TelegramLinkTokenResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Link.Should().StartWith($"https://t.me/{BotNameWithoutAt}?start=");
        result.Value.Link.Should().NotContain("@");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_Always_ReturnsExpireAtWithin10MinuteWindow()
    {
        // Arrange
        var command = new GenerateTelegramLinkCommand { UserId = Guid.NewGuid() };
        DateTime beforeCall = DateTime.UtcNow;

        // Act
        Result<TelegramLinkTokenResponseDto> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExpireAt.Should().BeCloseTo(beforeCall.AddMinutes(10), TimeSpan.FromSeconds(3));
    }
}
