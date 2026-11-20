using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Control.ArchitectureTests;

public class LayerTests : ServiceLayerArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
    protected override Assembly InfrastructureAssembly => BaseArchitectureTest.InfrastructureAssembly;
    protected override string ApplicationNamespace => "Control.Application";
    protected override string InfrastructureNamespace => "Control.Infrastructure";
    protected override string ApiNamespace => "Control.API";
}
