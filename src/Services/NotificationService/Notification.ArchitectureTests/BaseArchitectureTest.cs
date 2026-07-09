using System.Reflection;
using Notification.API.Controllers;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Persistence;

namespace Notification.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(NotificationDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(RemindersController).Assembly;
}
