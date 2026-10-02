using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Options;
using TelegramBotService.Infrastructure.Telegram;

namespace TelegramBotService.Endpoints;

/// <summary>
/// Minimal API endpoint that Telegram calls for every update.
/// URL: POST /webhook
/// Secured by the X-Telegram-Bot-Api-Secret-Token header (set at webhook registration).
/// </summary>
public static class TelegramWebhookEndpoint
{
    public static IEndpointRouteBuilder MapTelegramWebhook(this IEndpointRouteBuilder app)
    {
        app.MapPost("/webhook", async (
            [FromBody] JsonElement update,
            [FromServices] TelegramUpdateDispatcher dispatcher,
            [FromServices] Microsoft.Extensions.Options.IOptions<TelegramBotOptions> options,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            // Validate the secret token to prevent unauthorised calls
            if (!context.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerValue)
                || headerValue.ToString() != options.Value.BotSecretKey)
            {
                return Results.Unauthorized();
            }

            await dispatcher.HandleUpdateAsync(update, cancellationToken);
            return Results.Ok();
        })
        .WithTags("Telegram Webhook")
        .AllowAnonymous(); // Auth handled via secret header

        return app;
    }
}
