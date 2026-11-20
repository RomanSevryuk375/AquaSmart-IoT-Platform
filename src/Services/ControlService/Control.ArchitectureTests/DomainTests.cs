using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Control.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
