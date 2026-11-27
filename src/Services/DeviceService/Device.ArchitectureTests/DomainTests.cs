using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Device.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
