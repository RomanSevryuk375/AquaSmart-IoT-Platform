using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace BuildingBlocks.ArchitectureTestShared;

public abstract class DomainLayerArchitectureTestsBase
{
    protected abstract Assembly DomainAssembly { get; }
    protected abstract string ApplicationNamespace { get; }
    protected abstract string InfrastructureNamespace { get; }
    protected abstract string ApiNamespace { get; }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_Contracts()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Contracts")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_OtherBuildingBlocks()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("BuildingBlocks.Application")
            .And()
            .NotHaveDependencyOn("BuildingBlocks.Infrastructure")
            .And()
            .NotHaveDependencyOn("BuildingBlocks.Presentation")
            .And()
            .NotHaveDependencyOn("BuildingBlocks.IntegrationEvents")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApiLayer()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
