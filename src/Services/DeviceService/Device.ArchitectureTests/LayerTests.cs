using System.Reflection;
using BuildingBlocks.ArchitectureTestShared;
using FluentAssertions;
using NetArchTest.Rules;

namespace Device.ArchitectureTests;

public class LayerTests : ServiceLayerArchitectureTestsBase
{
    private const string MqttNetNamespace = "MQTTnet";

    protected override Assembly DomainAssembly => BaseArchitectureTest.DomainAssembly;
    protected override Assembly ApplicationAssembly => BaseArchitectureTest.ApplicationAssembly;
    protected override Assembly InfrastructureAssembly => BaseArchitectureTest.InfrastructureAssembly;
    protected override string ApplicationNamespace => "Device.Application";
    protected override string InfrastructureNamespace => "Device.Infrastructure";
    protected override string ApiNamespace => "Device.API";

    [Fact]
    public void DomainLayer_ShouldNotDependOn_MqttNet()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(MqttNetNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer must not depend on {MqttNetNamespace}. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_MqttNet()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(MqttNetNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application layer must not depend on {MqttNetNamespace}. Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
