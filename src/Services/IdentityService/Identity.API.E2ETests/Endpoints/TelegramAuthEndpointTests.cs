using System.Text;
using System.Text.Json;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Infrastructure.Data.Outbox;
using Identity.TestShared.Helpers;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace Identity.API.E2ETests.Endpoints;

public class TelegramAuthEndpointTests(E2ETestWebAppFactory factory) : BaseE2ETest(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GenerateTelegramLinkToken_Authenticated_ReturnsOkWithTelegramLink()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(UserContext.UserId)
            .WithEmail("tg.link@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        await UserManager.CreateAsync(user, "Password123!");

        // Act
        HttpResponseMessage response = await Client.PostAsync(
            $"{ApiConstants.Routes.Profiles}/telegram-link-token",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        TelegramLinkTokenResponseDto? content = await response.Content.ReadFromJsonAsync<TelegramLinkTokenResponseDto>();
        content.Should().NotBeNull();
        content!.Link.Should().StartWith($"https://t.me/{HmacTestHelper.TestBotName}?start=");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task GenerateTelegramLinkToken_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient unauthClient = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthClient.PostAsync(
            $"{ApiConstants.Routes.Profiles}/telegram-link-token",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_ValidHmacAndToken_ReturnsNoContentAndLinksChat()
    {
        // Arrange
        User user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("verify.success@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        await UserManager.CreateAsync(user, "Password123!");

        string token = Guid.NewGuid().ToString("N");
        IFusionCache cache = Factory.Services.GetRequiredService<IFusionCache>();
        await cache.SetAsync($"tg-link:{token}", user.Id.ToString(), TimeSpan.FromMinutes(10));

        const long chatId = 123456789L;
        var requestDto = new VerifyTelegramRequestDto
        {
            Token = token,
            ChatId = chatId
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        User? updatedUser = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        updatedUser.Should().NotBeNull();
        updatedUser!.TelegramChatId.Should().Be(chatId);

        // Cache token should be removed
        string? cachedUserId = await cache.GetOrDefaultAsync<string>($"tg-link:{token}");
        cachedUserId.Should().BeNull();

        // Outbox message should exist
        List<OutboxMessage> outboxMessages = await DbContext.OutboxMessages
            .AsNoTracking()
            .ToListAsync();

        outboxMessages.Should().ContainSingle(m => m.Type.Contains("TelegramAccountLinkedDomainEvent"));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_MissingHmacHeaders_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();
        var requestDto = new VerifyTelegramRequestDto
        {
            Token = "any-token",
            ChatId = 123456L
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_InvalidSignature_ReturnsUnauthorized()
    {
        // Arrange
        var requestDto = new VerifyTelegramRequestDto
        {
            Token = "any-token",
            ChatId = 123456L
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto,
            customSignature: "invalid-signature-hex-1234567890abcdef");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_ExpiredTimestamp_ReturnsUnauthorized()
    {
        // Arrange
        var requestDto = new VerifyTelegramRequestDto
        {
            Token = "any-token",
            ChatId = 123456L
        };

        long expiredTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto,
            customTimestamp: expiredTimestamp);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_InvalidOrExpiredToken_ReturnsNotFound()
    {
        // Arrange
        var requestDto = new VerifyTelegramRequestDto
        {
            Token = "non-existent-token",
            ChatId = 123456L
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task VerifyTelegram_AlreadyLinkedChatId_ReturnsConflict()
    {
        // Arrange
        const long alreadyLinkedChatId = 888777666L;

        User user1 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user1.tg@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        user1.LinkTelegramChat(alreadyLinkedChatId);
        await UserManager.CreateAsync(user1, "Password123!");

        User user2 = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("user2.tg@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        await UserManager.CreateAsync(user2, "Password123!");

        string token = Guid.NewGuid().ToString("N");
        IFusionCache cache = Factory.Services.GetRequiredService<IFusionCache>();
        await cache.SetAsync($"tg-link:{token}", user2.Id.ToString(), TimeSpan.FromMinutes(10));

        var requestDto = new VerifyTelegramRequestDto
        {
            Token = token,
            ChatId = alreadyLinkedChatId
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-verify",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task TelegramLogin_ValidHmacAndLinkedChatId_ReturnsOkAndAccessToken()
    {
        // Arrange
        const long linkedChatId = 999111222L;

        User user = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("tg.login@example.com")
            .WithSubscriptionId(Guid.Parse(SubscriptionType.Free))
            .Build();
        user.LinkTelegramChat(linkedChatId);
        await UserManager.CreateAsync(user, "Password123!");

        var requestDto = new TelegramLoginRequestDto
        {
            ChatId = linkedChatId
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-login",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        TelegramLoginResponseDto? content = await response.Content.ReadFromJsonAsync<TelegramLoginResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.ExpiresHours.Should().BeGreaterThan(0);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task TelegramLogin_UnlinkedChatId_ReturnsNotFound()
    {
        // Arrange
        var requestDto = new TelegramLoginRequestDto
        {
            ChatId = 123000999L
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-login",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task TelegramLogin_MissingHmacHeaders_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();
        var requestDto = new TelegramLoginRequestDto
        {
            ChatId = 123456L
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"{ApiConstants.Routes.Auth}/telegram-login",
            requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task TelegramLogin_InvalidSignature_ReturnsUnauthorized()
    {
        // Arrange
        var requestDto = new TelegramLoginRequestDto
        {
            ChatId = 123456L
        };

        // Act
        HttpResponseMessage response = await PostWithHmacAsync(
            $"/{ApiConstants.Routes.Auth}/telegram-login",
            requestDto,
            customSignature: "wrong-signature-123456");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpResponseMessage> PostWithHmacAsync(
        string path,
        object payload,
        long? customTimestamp = null,
        string? customSignature = null,
        string secretKey = HmacTestHelper.TestBotSecretKey)
    {
        HttpClient client = Factory.CreateClient();
        string json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var request = new HttpRequestMessage(HttpMethod.Post, normalizedPath)
        {
            Content = content
        };

        (string signature, string timestamp) = HmacTestHelper.GenerateHmacHeaders(
            "POST",
            normalizedPath,
            json,
            customTimestamp,
            secretKey);

        request.Headers.Add(ApiConstants.Headers.Timestamp, timestamp);
        request.Headers.Add(ApiConstants.Headers.HmacSignature, customSignature ?? signature);

        return await client.SendAsync(request);
    }
}
