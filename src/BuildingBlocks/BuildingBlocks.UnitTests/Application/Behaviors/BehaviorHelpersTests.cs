using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.UnitTests.Application.Behaviors;

public class BehaviorHelpersTests
{
    [Fact]
    public void CreateFailedResult_ForResult_ReturnsFailureResult()
    {
        // Arrange
        var error = Error.Validation("Test.Invalid", "Invalid input");

        // Act
        Result result = BehaviorHelpers.CreateFailedResult<Result>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void CreateFailedResult_ForGenericResult_ReturnsTypedFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "Missing item");

        // Act
        Result<string> result = BehaviorHelpers.CreateFailedResult<Result<string>>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void CreateFailedResult_ForUnsupportedType_ThrowsTypeInitializationOrUnauthorizedAccessException()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "Boom");

        // Act
        Action act = () => BehaviorHelpers.CreateFailedResult<string>(error);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>();
    }
}
