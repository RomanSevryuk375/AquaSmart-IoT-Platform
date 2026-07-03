// Ignore Spelling: Parameterless

using Contracts.Abstractions;

namespace Control.ArchitectureTests;

public class DomainTests : BaseArchitectureTest
{
    [Fact]
    public void Entities_ShouldHave_ParameterlessPrivateConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IEntity))
            .GetTypes();

        var failingTypes = new List<Type>();

        foreach (Type? type in entityTypes)
        {
            bool hasPrivateParameterlessConstructor = type.GetConstructors(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any(c => c.GetParameters().Length == 0);

            if (!hasPrivateParameterlessConstructor)
            {
                failingTypes.Add(type);
            }
        }

        failingTypes.Should().BeEmpty(
            "All entities must have a private/protected parameterless constructor for EF Core.");
    }

    [Fact]
    public void DomainEvents_ShouldImplement_IDomainEvent()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith("DomainEvent")
            .Should()
            .ImplementInterface(typeof(IDomainEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_ShouldBeSealed()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
