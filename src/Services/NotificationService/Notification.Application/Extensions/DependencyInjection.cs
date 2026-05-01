using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Interfaces;
using Notification.Application.Services;

namespace Notification.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IControllerAlertSender, ControllerAlertSender>();
        services.AddScoped<IEcosystemService, EcosystemService>();
        services.AddScoped<IMaintenanceLogService, MaintenanceLogService>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReminderProcessor, ReminderProcessor>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<ISensorAlertSender, SensorAlertSender>();
        services.AddScoped<ITelemetryAlertSender, TelemetryAlertSender>();
        services.AddScoped<IUnpublishedNoticeProcessor, UnpublishedNoticeProcessor>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
