using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Telemetry.ArchitectureTests;

public class DomainTests : DomainArchitectureTestsBase
{
    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
}
