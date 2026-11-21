using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Notification.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
