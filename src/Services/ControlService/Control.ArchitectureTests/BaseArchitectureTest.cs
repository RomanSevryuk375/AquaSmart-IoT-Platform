using System.Reflection;
using Control.API.Controllers;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Control.Infrastructure.Persistence;

namespace Control.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(AutomationRule).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(SystemDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(AutomationRulesController).Assembly;
}
