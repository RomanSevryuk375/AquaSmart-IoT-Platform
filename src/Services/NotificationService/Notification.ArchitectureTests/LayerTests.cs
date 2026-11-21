using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Notification.ArchitectureTests;

public class LayerTests : ServiceLayerArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
    protected override Assembly InfrastructureAssembly => BaseArchitectureTest.InfrastructureAssembly;
    protected override string ApplicationNamespace => "Notification.Application";
    protected override string InfrastructureNamespace => "Notification.Infrastructure";
    protected override string ApiNamespace => "Notification.API";
}
