using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Control.ArchitectureTests;

public class ApplicationTests : ApplicationArchitectureTestsBase
{
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
}
