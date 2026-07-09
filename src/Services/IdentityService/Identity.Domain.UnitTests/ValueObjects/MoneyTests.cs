using Contracts.Results;
using FluentAssertions;
using IdentityService.Domain.ValueObjects;
using Xunit;

namespace Identity.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNegativeAmount_ReturnsFailure(decimal negativeAmount)
    {
        // Act
        var result = Money.Create(negativeAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.Invalid");
        result.Error.Message.Should().Be("Amount cannot be negative.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(19.99)]
    [InlineData(1000000)]
    public void Create_WithValidAmount_ReturnsSuccess(decimal validAmount)
    {
        // Act
        var result = Money.Create(validAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(validAmount);
    }
}
