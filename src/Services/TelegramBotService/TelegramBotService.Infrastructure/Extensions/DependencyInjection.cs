using Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotService.Infrastructure.Adapters;
using TelegramBotService.Infrastructure.HttpClients;
using TelegramBotService.Infrastructure.Sessions;
using TelegramBotService.Infrastructure.Telegram;

namespace TelegramBotService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBotInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddRedisCache(configuration)
            .AddTokenStore()
            .AddSessionStore()
            .AddHttpClients(configuration)
            .AddTelegramServices();
    }

    private static IServiceCollection AddRedisCache(
        this IServiceCollection services, IConfiguration configuration)
    {
        string redis = configuration.GetConnectionString("Redis")
                       ?? throw new InvalidOperationException("Redis connection string is required.");

        services.AddStackExchangeRedisCache(opt => opt.Configuration = redis);
        return services;
    }

    private static IServiceCollection AddTokenStore(this IServiceCollection services)
    {
        services.AddSingleton<RedisTokenStore>();
        return services;
    }

    private static IServiceCollection AddSessionStore(this IServiceCollection services)
    {
        services.AddScoped<IFsmSessionStore, RedisFsmSessionStore>();
        return services;
    }

    private static IServiceCollection AddHttpClients(
        this IServiceCollection services, IConfiguration configuration)
    {
        string gatewayUrl = configuration["Services:GatewayUrl"]
                            ?? throw new InvalidOperationException("Gateway URL is required.");

        string identityUrl = configuration["Services:IdentityUrl"]
                             ?? throw new InvalidOperationException("Identity URL is required.");

        // Identity S2S client (HMAC signed) — uses existing IdentityHttpClient
        services.AddHttpClient<IdentityHttpClient>(c => c.BaseAddress = new Uri(identityUrl));

        // Adapter wrapping IdentityHttpClient behind IIdentityClient
        services.AddScoped<IIdentityClient, IdentityClientAdapter>();

        // Notification API client (Bearer JWT via Gateway)
        services.AddHttpClient<INotificationApiClient, NotificationApiClient>(
            c => c.BaseAddress = new Uri(gatewayUrl));

        // Control API client (Bearer JWT via Gateway)
        services.AddHttpClient<IControlApiClient, ControlApiClient>(
            c => c.BaseAddress = new Uri(gatewayUrl));

        // Telegram response service (raw Telegram Bot API calls)
        services.AddHttpClient<ITelegramResponseService, TelegramResponseService>();

        return services;
    }

    private static IServiceCollection AddTelegramServices(this IServiceCollection services)
    {
        services.AddScoped<TelegramUpdateDispatcher>();
        return services;
    }
}
