using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BuildingBlocks.ArchitectureTestShared;

public abstract class ServiceLayerArchitectureTestsBase : DomainLayerArchitectureTestsBase
{
    protected abstract Assembly ApplicationAssembly { get; }
    protected abstract Assembly InfrastructureAssembly { get; }

    [Fact]
    public void ApplicationLayer_ShouldHaveDependencyOn_BuildingBlocksApplication()
    {
        ApplicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should()
            .Contain("BuildingBlocks.Application");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_ApiLayer()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void InfrastructureLayer_ShouldHaveDependencyOn_BuildingBlocksInfrastructure()
    {
        InfrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should()
            .Contain("BuildingBlocks.Infrastructure");
    }
}
