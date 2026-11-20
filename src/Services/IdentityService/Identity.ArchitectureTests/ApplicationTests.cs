using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Identity.ArchitectureTests;

public class ApplicationTests : ApplicationArchitectureTestsBase
{
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
}
