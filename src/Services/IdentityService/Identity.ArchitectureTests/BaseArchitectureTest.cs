using System.Reflection;
using IdentityService.API.Controllers;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure;

namespace Identity.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(IdentityDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(AuthController).Assembly;
}
