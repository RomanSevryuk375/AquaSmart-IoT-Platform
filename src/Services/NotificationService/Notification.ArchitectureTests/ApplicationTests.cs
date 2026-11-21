using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;

namespace Notification.ArchitectureTests;

public class ApplicationTests : ApplicationArchitectureTestsBase
{
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
}
