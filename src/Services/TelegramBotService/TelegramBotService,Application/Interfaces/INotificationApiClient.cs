using DTOs;

namespace Interfaces;

/// <summary>
/// HTTP client contract for NotificationService (accessed via API Gateway with user Bearer JWT).
/// </summary>
public interface INotificationApiClient
{
    // ── Reminders ─────────────────────────────────────────────────────────────

    /// <summary>Fetch all reminders for the authenticated user.</summary>
    public Task<IReadOnlyList<ReminderDto>> GetAllRemindersAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    /// <summary>Create a new recurring reminder.</summary>
    public Task<Guid> CreateReminderAsync(
        long chatId,
        CreateReminderRequest dto,
        CancellationToken cancellationToken = default);

    /// <summary>Mark a reminder as completed (triggers next due-date calculation).</summary>
    public Task<DateTime> CompleteReminderAsync(
        long chatId,
        Guid reminderId,
        CancellationToken cancellationToken = default);

    // ── Maintenance Logs ──────────────────────────────────────────────────────

    /// <summary>Submit a new maintenance log entry.</summary>
    public Task<Guid> CreateMaintenanceLogAsync(
        long chatId,
        CreateMaintenanceLogRequest dto,
        CancellationToken cancellationToken = default);

    // ── Notifications ─────────────────────────────────────────────────────────

    /// <summary>Mark a notification as read (removes the inline button).</summary>
    public Task MarkNotificationAsReadAsync(
        long chatId,
        Guid notificationId,
        CancellationToken cancellationToken = default);
}
