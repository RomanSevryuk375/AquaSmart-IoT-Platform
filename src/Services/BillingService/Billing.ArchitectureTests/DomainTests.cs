using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Billing.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
