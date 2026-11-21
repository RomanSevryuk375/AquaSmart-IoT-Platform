using System.Reflection;
using Notification.API.Endpoints.Reminders;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(User).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(ProcessUnpublishedNoticesCommand).Assembly;
    internal static readonly Assembly InfrastructureAssembly = typeof(NotificationDbContext).Assembly;
    internal static readonly Assembly ApiAssembly = typeof(AddReminderEndpoint).Assembly;
}
