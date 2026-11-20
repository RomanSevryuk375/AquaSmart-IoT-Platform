using System.Reflection;
using Device.API.Endpoints.Relays;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Infrastructure.Persistence;

namespace Device.ArchitectureTests;

public abstract class BaseArchitectureTest
{
    internal static readonly Assembly DomainAssembly = typeof(Controller).Assembly;
    internal static readonly Assembly ApplicationAssembly = typeof(IDeviceSecurityService).Assembly;
    internal static readonly Assembly InfrastructureAssembly = typeof(DeviceDbContext).Assembly;
    internal static readonly Assembly ApiAssembly = typeof(AddRelayEndpoint).Assembly;
}
