using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Billing.ArchitectureTests;

public class LayerTests : DomainLayerArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override string ApplicationNamespace => "Billing.Application";
    protected override string InfrastructureNamespace => "Billing.Infrastructure";
    protected override string ApiNamespace => "Billing.API";
}
