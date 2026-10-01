using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Presentation.Constants;
using Microsoft.Extensions.Options;
using Options;

namespace TelegramBotService.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for IdentityService, accessed via M2M secret token.
/// Used to verify and login user for the bot.
/// </summary>
internal class IdentityHttpClient(
    HttpClient httpClient,
    IOptions<TelegramBotOptions> options)
{
    private readonly string _secretKey = options.Value.BotSecretKey;

    public async Task<bool> VerifyLinkTokenAsync(
        string token,
        long chatId,
        CancellationToken cancellationToken)
    {
        var payload = new { Token = token, ChatId = chatId };
        return await SendSignedPostAsync(
            $"{ApiConstants.Routes.Auth}/telegram-verify",
            payload,
            cancellationToken);
    }

    public async Task<string?> LoginByChatIdAsync(
        long chatId,
        CancellationToken cancellationToken)
    {
        var payload = new { ChatId = chatId };
        string path = $"{ApiConstants.Routes.Auth}/telegram-login";

        HttpRequestMessage request = CreateSignedRequest(HttpMethod.Post, path, payload);
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using JsonDocument doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        return doc.RootElement.GetProperty("accessToken").GetString();
    }

    private async Task<bool> SendSignedPostAsync(
        string path, object payload, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateSignedRequest(HttpMethod.Post, path, payload);
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private HttpRequestMessage CreateSignedRequest(HttpMethod method, string path, object payload)
    {
        string bodyJson = JsonSerializer.Serialize(payload);
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        string canonicalString = $"{method.Method}\n{path}\n{timestamp}\n{bodyJson}";
        string signature = ComputeHmac(canonicalString, _secretKey);

        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        request.Headers.Add(ApiConstants.Headers.Timestamp, timestamp);
        request.Headers.Add(ApiConstants.Headers.HmacSignature, signature);

        return request;
    }
    private static string ComputeHmac(string data, string secret)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
