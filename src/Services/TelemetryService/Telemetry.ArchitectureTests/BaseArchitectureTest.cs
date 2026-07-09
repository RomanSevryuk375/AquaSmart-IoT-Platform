using System.Reflection;
using Telemetry.API.Controllers;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    protected static readonly Assembly DomainAssembly = typeof(Ecosystem).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(TelemetryDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(TelemetryDataController).Assembly;
}
