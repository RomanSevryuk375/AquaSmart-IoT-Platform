using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Telemetry.ArchitectureTests;

public class ApplicationTests : ApplicationArchitectureTestsBase
{
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
}
