using System.Reflection;
using Telemetry.API.Endpoints;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(Ecosystem).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(AddTelemetryBatchCommand).Assembly;
    internal static readonly Assembly InfrastructureAssembly = typeof(TelemetryDbContext).Assembly;
    internal static readonly Assembly ApiAssembly = typeof(GetRawDataEndpoint).Assembly;
}
