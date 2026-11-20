using System.Reflection;
using Control.API.Endpoints.AutomationRules;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Infrastructure.Persistence;

namespace Control.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(AutomationRule).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(IRuleBoundRequest).Assembly;
    internal static readonly Assembly InfrastructureAssembly = typeof(ControlDbContext).Assembly;
    internal static readonly Assembly ApiAssembly = typeof(AddConditionEndpoint).Assembly;
}
