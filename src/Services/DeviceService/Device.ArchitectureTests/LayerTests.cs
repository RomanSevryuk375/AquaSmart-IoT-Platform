using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Device.ArchitectureTests;

public class LayerTests : ServiceLayerArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
    protected override Assembly InfrastructureAssembly => BaseArchitectureTest.InfrastructureAssembly;
    protected override string ApplicationNamespace => "Device.Application";
    protected override string InfrastructureNamespace => "Device.Infrastructure";
    protected override string ApiNamespace => "Device.API";
}
