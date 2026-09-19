using System.Text.Json;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Infrastructure.Data.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Data.Interceptors;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDomainEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDomainEvents(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        ClearDomainEvents(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        RemoveAddedOutboxMessages(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RemoveAddedOutboxMessages(eventData.Context);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private static void ConvertDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var entitiesWithEvents = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var outboxMessages = entitiesWithEvents
            .SelectMany(root => root.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = NewId.NextGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            })
            .ToList();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }

    private static void ClearDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var entities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(x => x.Entity);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }

    private static void RemoveAddedOutboxMessages(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var addedOutboxEntries = dbContext.ChangeTracker
            .Entries<OutboxMessage>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in addedOutboxEntries)
        {
            entry.State = EntityState.Detached;
        }
    }
}
