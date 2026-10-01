using Handlers.Callbacks;
using Handlers.Commands;
using Handlers.Fsm;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddBotApplication(this IServiceCollection services)
    {
        // Command handlers
        services.AddScoped<StartCommandHandler>();
        services.AddScoped<ShowRemindersHandler>();

        // FSM handlers
        services.AddScoped<AddLogFsmHandler>();
        services.AddScoped<AddReminderFsmHandler>();

        // Callback handlers
        services.AddScoped<MarkNotificationReadCallbackHandler>();
        services.AddScoped<CompleteReminderCallbackHandler>();
        services.AddScoped<CancelFlowCallbackHandler>();

        return services;
    }
}
