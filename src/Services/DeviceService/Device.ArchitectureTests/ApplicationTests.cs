using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Device.ArchitectureTests;

public class ApplicationTests : ApplicationArchitectureTestsBase
{
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
}
