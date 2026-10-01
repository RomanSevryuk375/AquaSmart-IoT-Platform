using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Presentation.Constants;
using DTOs;
using Interfaces;
using Microsoft.Extensions.Logging;
using TelegramBotService.Infrastructure.Sessions;

namespace TelegramBotService.Infrastructure.HttpClients;

/// <summary>
/// HTTP client for ControlService, accessed via the API Gateway with user Bearer JWT.
/// Used to fetch ecosystems for the inline keyboard ecosystem-selection step.
/// </summary>
internal sealed class ControlApiClient(
    HttpClient httpClient,
    RedisTokenStore tokenStore,
    ILogger<ControlApiClient> logger) : IControlApiClient
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<EcosystemDto>> GetUserEcosystemsAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        string? token = await tokenStore.GetTokenAsync(chatId, cancellationToken);

        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("No JWT token for chatId={ChatId} when fetching ecosystems", chatId);
            return [];
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiConstants.Routes.Ecosystems);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<EcosystemDto>>(_jsonOpts, cancellationToken)
            ?? [];
    }
}
