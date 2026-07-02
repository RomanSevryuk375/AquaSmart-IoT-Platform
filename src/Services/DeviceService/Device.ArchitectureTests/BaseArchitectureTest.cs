using System.Reflection;
using Device.API.Controllers;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Device.Infrastructure.Persistence;

namespace Device.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(Controller).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(SystemDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(ControllersController).Assembly;
}
