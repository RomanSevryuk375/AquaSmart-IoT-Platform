using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Identity.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
