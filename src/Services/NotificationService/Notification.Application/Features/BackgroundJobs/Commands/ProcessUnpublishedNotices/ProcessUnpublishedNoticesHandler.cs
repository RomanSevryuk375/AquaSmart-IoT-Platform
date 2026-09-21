// Ignore Spelling: Tg

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Dapper;
using MassTransit;
using MediatR;
using Notification.Application.InternalEvents;

namespace Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public sealed class ProcessUnpublishedNoticesHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IPublishEndpoint publishEndpoint) : IRequestHandler<ProcessUnpublishedNoticesCommand, Result>
{
    private const int BatchSize = 100;
    private const int MaxRetryCount = 5;

    private const string SelectBatchSql = """
        SELECT 
            id AS Id, 
            user_id AS UserId, 
            message AS Message
        FROM notifications
        WHERE is_published = false 
          AND retry_count < @MaxRetryCount
        ORDER BY created_at
        LIMIT @BatchSize
        FOR UPDATE SKIP LOCKED;
        """;

    private const string GetUsersSql = """
        SELECT 
            id AS Id, 
            email AS Email, 
            email_enable AS EmailEnable, 
            tg_enable AS TgEnable, 
            telegram_chat_id AS TelegramChatId, 
            is_notify_enabled AS IsNotifyEnabled
        FROM users
        WHERE id = ANY(@UserIds);
        """;

    private const string UpdateSuccessSql = """
        UPDATE notifications
        SET is_published = true,
            published_at = @PublishedAt
        WHERE id = ANY(@Ids);
        """;

    private const string UpdateFailureSql = """
        UPDATE notifications
        SET retry_count = retry_count + 1,
            failure_reason = @FailureReason
        WHERE id = ANY(@Ids);
        """;

    public async Task<Result> Handle(ProcessUnpublishedNoticesCommand request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(cancellationToken);
        }
        else
        {
            connection.Open();
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            using IDbTransaction transaction = connection.BeginTransaction();

            var rawNotifications = (await connection.QueryAsync<UnpublishedNotificationDto>(
                new CommandDefinition(
                    SelectBatchSql,
                    new { BatchSize, MaxRetryCount },
                    transaction: transaction,
                    cancellationToken: cancellationToken))).ToList();

            if (rawNotifications.Count == 0)
            {
                transaction.Commit();
                break;
            }

            Guid[] userIds = rawNotifications.Select(n => n.UserId).Distinct().ToArray();
            Dictionary<Guid, UserNotificationSettingsDto> usersDict = await FetchUsersDictionaryAsync(
                connection, userIds, transaction, cancellationToken);

            var successfulIds = new List<Guid>();
            var failedNotifications = new List<FailedNotificationDto>();

            foreach (UnpublishedNotificationDto notification in rawNotifications)
            {
                if (!usersDict.TryGetValue(notification.UserId, out UserNotificationSettingsDto? user)
                    || !user.IsNotifyEnabled)
                {
                    failedNotifications.Add(new FailedNotificationDto(notification.Id, ErrorMessages.NotificationProvider.UserDisabledOrNotFound));
                    continue;
                }

                if (!user.TgEnable && !user.EmailEnable)
                {
                    failedNotifications.Add(new FailedNotificationDto(notification.Id, ErrorMessages.NotificationProvider.NoActiveChannels));
                    continue;
                }

                if (user.TgEnable && user.TelegramChatId is null)
                {
                    failedNotifications.Add(new FailedNotificationDto(notification.Id, ErrorMessages.NotificationProvider.TgChatIdMissing));
                    continue;
                }

                try
                {
                    await DispatchNotificationAsync(notification, user, cancellationToken);
                    successfulIds.Add(notification.Id);
                }
                catch (Exception ex)
                {
                    failedNotifications.Add(new FailedNotificationDto(notification.Id, $"Dispatch error: {ex.Message}"));
                }
            }

            await MarkNotificationsAsPublishedAsync(connection, transaction, successfulIds, cancellationToken);
            await MarkNotificationsAsFailedAsync(connection, transaction, failedNotifications, cancellationToken);

            transaction.Commit();

            if (rawNotifications.Count < BatchSize)
            {
                break;
            }
        }

        return Result.Success();
    }

    private static async Task<Dictionary<Guid, UserNotificationSettingsDto>> FetchUsersDictionaryAsync(
        IDbConnection connection,
        Guid[] userIds,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        IEnumerable<UserNotificationSettingsDto> users = await connection.QueryAsync<UserNotificationSettingsDto>(
            new CommandDefinition(
                GetUsersSql,
                new { UserIds = userIds },
                transaction: transaction,
                cancellationToken: cancellationToken));

        return users.ToDictionary(u => u.Id);
    }

    private async Task DispatchNotificationAsync(
        UnpublishedNotificationDto notification,
        UserNotificationSettingsDto user,
        CancellationToken cancellationToken)
    {
        if (user.TgEnable && user.TelegramChatId is { } chatId)
        {
            await publishEndpoint.Publish(new SendTelegramCommand
            {
                NotificationId = notification.Id,
                ChatId = chatId,
                Message = notification.Message
            }, cancellationToken);
        }

        if (user.EmailEnable)
        {
            await publishEndpoint.Publish(new SendEmailCommand
            {
                NotificationId = notification.Id,
                Email = user.Email,
                Message = notification.Message
            }, cancellationToken);
        }
    }

    private static async Task MarkNotificationsAsPublishedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> successfulNotificationIds,
        CancellationToken cancellationToken)
    {
        if (successfulNotificationIds.Count == 0)
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            UpdateSuccessSql,
            new { PublishedAt = DateTime.UtcNow, Ids = successfulNotificationIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    private static async Task MarkNotificationsAsFailedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<FailedNotificationDto> failedNotifications,
        CancellationToken cancellationToken)
    {
        if (failedNotifications.Count == 0)
        {
            return;
        }

        foreach (IGrouping<string, FailedNotificationDto> group in failedNotifications.GroupBy(x => x.FailureReason))
        {
            await connection.ExecuteAsync(new CommandDefinition(
                UpdateFailureSql,
                new { FailureReason = group.Key, Ids = group.Select(x => x.Id).ToArray() },
                transaction: transaction,
                cancellationToken: cancellationToken));
        }
    }

    [SuppressMessage("SonarAnalyzer.CSharp", "S3459", Justification = "Populated by Dapper via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144", Justification = "Populated by Dapper via reflection")]
    private sealed class UnpublishedNotificationDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    [SuppressMessage("SonarAnalyzer.CSharp", "S3459", Justification = "Populated by Dapper via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144", Justification = "Populated by Dapper via reflection")]
    private sealed class UserNotificationSettingsDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public bool EmailEnable { get; init; }
        public bool TgEnable { get; init; }
        public long? TelegramChatId { get; init; }
        public bool IsNotifyEnabled { get; init; }
    }

    private sealed record FailedNotificationDto(Guid Id, string FailureReason);
}
