using BuildingBlocks.Infrastructure.Extensions;
using Extensions;
using Options;
using TelegramBotService.Endpoints;
using TelegramBotService.Infrastructure.Extensions;

await MicroserviceRunner.RunAsync("AquaSmart.TelegramBotService", args, builder =>
{
    // Configuration
    builder.Services.Configure<TelegramBotOptions>(
        builder.Configuration.GetSection(TelegramBotOptions.SectionName));

    // Application layer
    builder.Services.AddBotApplication();

    // Infrastructure layer (Redis, HttpClients, Dispatcher)
    builder.Services.AddBotInfrastructure(builder.Configuration);
},
configureApp: app =>
{
    app.MapTelegramWebhook();
});
