using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Identity.ArchitectureTests;

public class LayerTests : ServiceLayerArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
    protected override Assembly InfrastructureAssembly => BaseArchitectureTest.InfrastructureAssembly;
    protected override string ApplicationNamespace => "IdentityService.Application";
    protected override string InfrastructureNamespace => "IdentityService.Infrastructure";
    protected override string ApiNamespace => "IdentityService.API";
}
