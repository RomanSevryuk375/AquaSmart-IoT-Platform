using System.Data.Common;
using System.Text.Json;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Data.Outbox;

public sealed class OutboxMessageProcessorService<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxMessageProcessorService<TDbContext>> logger,
    IOptions<OutboxOptions>? options = null)
    where TDbContext : DbContext
{
    private readonly OutboxOptions _options = options?.Value ?? new OutboxOptions();

    public async Task<Result> ProcessAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(OutboxMessage));
        string tableName = entityType?.GetTableName() ?? "outbox_messages";
        string? schema = entityType?.GetSchema();
        string fullTableName = string.IsNullOrWhiteSpace(schema)
            ? $"\"{tableName}\""
            : $"\"{schema}\".\"{tableName}\"";

        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        DbConnection connection = dbContext.Database.GetDbConnection();
        DbTransaction? dbTransaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();

        DateTime now = DateTime.UtcNow;

        string selectSql = $"""
            SELECT 
              id AS Id, 
              type AS Type, 
              content AS Content, 
              occurred_on_utc AS OccurredOnUtc,
              retry_count AS RetryCount,
              next_retry_on_utc AS NextRetryOnUtc
            FROM {fullTableName}
            WHERE processed_on_utc IS NULL
              AND (next_retry_on_utc IS NULL OR next_retry_on_utc <= @Now)
            ORDER BY occurred_on_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED
            """;

#pragma warning disable S2077 // Dynamic SQL is safe here because tableName and schema are retrieved from EF Core metadata
        var messages = (await connection.QueryAsync<OutboxMessage>(
            selectSql,
            new { Now = now, BatchSize = _options.BatchSize },
            transaction: dbTransaction)).ToList();
#pragma warning restore S2077

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }

        foreach (OutboxMessage message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type);
                if (type is null)
                {
                    logger.LogWarning("Type {MessageType} not found for outbox message {MessageId}. Marking as poison message.",
                        message.Type, message.Id);
                    message.Error = $"Type {message.Type} not found.";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                object? deserialized;
                try
                {
                    deserialized = JsonSerializer.Deserialize(message.Content, type);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogWarning(jsonEx, "Failed to deserialize outbox message {MessageId}. Marking as poison message.", message.Id);
                    message.Error = $"JSON deserialization error: {jsonEx.Message}";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                if (deserialized is not IDomainEvent domainEvent)
                {
                    logger.LogWarning("Content of outbox message {MessageId} is not an IDomainEvent. Marking as poison message.", message.Id);
                    message.Error = "Content is not an IDomainEvent.";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                await publisher.Publish(domainEvent, cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                if (message.RetryCount >= _options.MaxRetries)
                {
                    logger.LogError(ex, "Outbox message {MessageId} exceeded max retries ({MaxRetries}). Marking as failed.",
                        message.Id, _options.MaxRetries);
                    message.Error = ex.ToString();
                    message.ProcessedOnUtc = DateTime.UtcNow;
                }
                else
                {
                    double delaySeconds = Math.Pow(2, message.RetryCount);
                    message.NextRetryOnUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
                    message.Error = ex.ToString();
                    logger.LogWarning(ex, "Failed to process outbox message {MessageId} (attempt {RetryCount}/{MaxRetries}). Next retry at {NextRetryOnUtc}",
                        message.Id, message.RetryCount, _options.MaxRetries, message.NextRetryOnUtc);
                }
            }
        }

        string updateSql = $"""
            UPDATE {fullTableName}
            SET processed_on_utc = @ProcessedOnUtc, 
                error = @Error,
                retry_count = @RetryCount,
                next_retry_on_utc = @NextRetryOnUtc
            WHERE id = @Id
            """;

#pragma warning disable S2077
        await connection.ExecuteAsync(
            updateSql,
            messages.Select(m => new { m.ProcessedOnUtc, m.Error, m.RetryCount, m.NextRetryOnUtc, m.Id }),
            transaction: dbTransaction);
#pragma warning restore S2077

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CleanupAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(OutboxMessage));
        string tableName = entityType?.GetTableName() ?? "outbox_messages";
        string? schema = entityType?.GetSchema();
        string fullTableName = string.IsNullOrWhiteSpace(schema)
            ? $"\"{tableName}\""
            : $"\"{schema}\".\"{tableName}\"";

        DateTime threshold = DateTime.UtcNow.AddDays(-_options.RetentionDays);

        DbConnection connection = dbContext.Database.GetDbConnection();

        string deleteSql = $"""
            DELETE FROM {fullTableName}
            WHERE processed_on_utc IS NOT NULL
              AND processed_on_utc < @Threshold
            """;

#pragma warning disable S2077
        int deletedCount = await connection.ExecuteAsync(deleteSql, new { Threshold = threshold });
#pragma warning restore S2077

        if (deletedCount > 0)
        {
            logger.LogInformation("Outbox cleanup removed {Count} processed messages older than {Threshold} from {Table}",
                deletedCount, threshold, fullTableName);
        }

        return Result.Success();
    }
}
