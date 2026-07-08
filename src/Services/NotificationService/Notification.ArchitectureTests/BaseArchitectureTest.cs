using System.Reflection;
using Notification.API.Controllers;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using Notification.Infrastructure;

namespace Notification.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(SystemDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(RemindersController).Assembly;
}
