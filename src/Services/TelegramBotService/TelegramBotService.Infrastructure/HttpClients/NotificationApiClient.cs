// Ignore Spelling: dto

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
/// HTTP client for NotificationService, accessed through the API Gateway.
/// Automatically attaches the user's Bearer JWT stored in Redis.
/// </summary>
internal sealed class NotificationApiClient(
    HttpClient httpClient,
    RedisTokenStore tokenStore,
    ILogger<NotificationApiClient> logger) : INotificationApiClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── Reminders ─────────────────────────────────────────────────

    public async Task<IReadOnlyList<ReminderDto>> GetAllRemindersAsync(
        long chatId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = await CreateAuthorizedRequestAsync(
            HttpMethod.Get,
            $"{ApiConstants.Routes.Reminders}?take=50",
            chatId,
            cancellationToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<ReminderDto>>(JsonOpts, cancellationToken)
            ?? [];
    }

    public async Task<Guid> CreateReminderAsync(
        long chatId, CreateReminderRequest dto, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = await CreateAuthorizedJsonRequestAsync(
            HttpMethod.Post,
            ApiConstants.Routes.Reminders,
            dto,
            chatId,
            cancellationToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
    }

    public async Task<DateTime> CompleteReminderAsync(
        long chatId, Guid reminderId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = await CreateAuthorizedRequestAsync(
            HttpMethod.Patch,
            $"{ApiConstants.Routes.Reminders}/{reminderId}/complete",
            chatId,
            cancellationToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return DateTime.UtcNow;
    }

    // ── Maintenance Logs ──────────────────────────────────────────

    public async Task<Guid> CreateMaintenanceLogAsync(
        long chatId, CreateMaintenanceLogRequest dto, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = await CreateAuthorizedJsonRequestAsync(
            HttpMethod.Post,
            ApiConstants.Routes.MaintenanceLogs,
            dto,
            chatId,
            cancellationToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: cancellationToken);
    }

    // ── Notifications ─────────────────────────────────────────────

    public async Task MarkNotificationAsReadAsync(
        long chatId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = await CreateAuthorizedRequestAsync(
            HttpMethod.Put,
            $"{ApiConstants.Routes.Notifications}/{notificationId}/read",
            chatId,
            cancellationToken);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    // ── Helpers ───────────────────────────────────────────────────

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(
        HttpMethod method,
        string path,
        long chatId,
        CancellationToken cancellationToken)
    {
        string? token = await tokenStore.GetTokenAsync(chatId, cancellationToken);
        var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            logger.LogWarning("No JWT token found for chatId={ChatId} on path {Path}", chatId, path);
        }

        return request;
    }

    private async Task<HttpRequestMessage> CreateAuthorizedJsonRequestAsync<T>(
        HttpMethod method,
        string path,
        T body,
        long chatId,
        CancellationToken cancellationToken)
    {
        string? token = await tokenStore.GetTokenAsync(chatId, cancellationToken);
        var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body)
        };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }
}
