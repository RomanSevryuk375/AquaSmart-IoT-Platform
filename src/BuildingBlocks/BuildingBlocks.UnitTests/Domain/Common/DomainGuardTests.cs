using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.UnitTests.Domain.Common;

public class DomainGuardTests
{
    private sealed class DummyEntity
    {
        public Guid Id { get; init; } = Guid.Empty;
    }

    private enum SampleKind
    {
        Valid = 1
    }

    [Fact]
    public void RequireNotEmpty_WithEmptyGuid_ReturnsFailure()
    {
        // Act
        Result result = DomainGuard.RequireNotEmpty<DummyEntity>(Guid.Empty, "EntityId");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DummyEntity.Invalid");
        result.Error.Message.Should().Be("EntityId cannot be empty.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void RequireNotEmpty_WithValidGuid_ReturnsSuccess()
    {
        // Act
        Result result = DomainGuard.RequireNotEmpty<DummyEntity>(Guid.NewGuid(), "EntityId");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireText_WithNullOrWhitespace_ReturnsFailure(string? value)
    {
        // Act
        Result<string> result = DomainGuard.RequireText<DummyEntity>(value, 10, "Name");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DummyEntity.Invalid");
        result.Error.Message.Should().Be("Name is required.");
    }

    [Fact]
    public void RequireText_WhenExceedingMaxLength_ReturnsFailure()
    {
        // Act
        Result<string> result = DomainGuard.RequireText<DummyEntity>("123456", 5, "Name");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Name cannot exceed 5 characters.");
    }

    [Fact]
    public void RequireText_WithValidText_TrimsAndReturnsSuccess()
    {
        // Act
        Result<string> result = DomainGuard.RequireText<DummyEntity>("  hello  ", 10, "Name");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OptionalText_WithNullOrWhitespace_ReturnsNullSuccess(string? value)
    {
        // Act
        Result<string?> result = DomainGuard.OptionalText<DummyEntity>(value, 10, "Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void OptionalText_WhenExceedingMaxLength_ReturnsFailure()
    {
        // Act
        Result<string?> result = DomainGuard.OptionalText<DummyEntity>("123456", 5, "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Description cannot exceed 5 characters.");
    }

    [Fact]
    public void OptionalText_WithValidText_TrimsAndReturnsSuccess()
    {
        // Act
        Result<string?> result = DomainGuard.OptionalText<DummyEntity>("  notes  ", 10, "Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("notes");
    }

    [Fact]
    public void RequireEnum_WithUndefinedValue_ReturnsFailure()
    {
        // Act
        Result result = DomainGuard.RequireEnum<DummyEntity, SampleKind>((SampleKind)999, "Status");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Invalid Status.");
    }

    [Fact]
    public void RequireEnum_WithDefinedValue_ReturnsSuccess()
    {
        // Act
        Result result = DomainGuard.RequireEnum<DummyEntity, SampleKind>(SampleKind.Valid, "Status");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RequireNumericGuards_ValidateNegativePositiveAndRange()
    {
        // Assert
        DomainGuard.RequireNonNegative<DummyEntity>(-1m, "Amount").IsFailure.Should().BeTrue();
        DomainGuard.RequireNonNegative<DummyEntity>(0m, "Amount").IsSuccess.Should().BeTrue();
        DomainGuard.RequireNonNegative<DummyEntity>(-1, "Count").IsFailure.Should().BeTrue();
        DomainGuard.RequireNonNegative<DummyEntity>(0, "Count").IsSuccess.Should().BeTrue();

        DomainGuard.RequirePositive<DummyEntity>(0m, "Price").IsFailure.Should().BeTrue();
        DomainGuard.RequirePositive<DummyEntity>(0.1m, "Price").IsSuccess.Should().BeTrue();
        DomainGuard.RequirePositive<DummyEntity>(0, "Days").IsFailure.Should().BeTrue();
        DomainGuard.RequirePositive<DummyEntity>(1, "Days").IsSuccess.Should().BeTrue();
        DomainGuard.RequirePositive<DummyEntity>(0L, "ChatId").IsFailure.Should().BeTrue();
        DomainGuard.RequirePositive<DummyEntity>(1L, "ChatId").IsSuccess.Should().BeTrue();

        DomainGuard.RequireInRange<DummyEntity>(0, 1, 10, "Interval").IsFailure.Should().BeTrue();
        DomainGuard.RequireInRange<DummyEntity>(11, 1, 10, "Interval").IsFailure.Should().BeTrue();
        DomainGuard.RequireInRange<DummyEntity>(5, 1, 10, "Interval").IsSuccess.Should().BeTrue();
        DomainGuard.RequireInRange<DummyEntity>(10.5m, 1m, 10m, "Rate").IsFailure.Should().BeTrue();
        DomainGuard.RequireInRange<DummyEntity>(5.5m, 1m, 10m, "Rate").IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ListBasedChecks_CollectErrorsAndCreateAggregatedFailure()
    {
        // Arrange
        var errors = new List<string>();
        Result<int> failedVo = Result<int>.Failure(Error.Validation<DummyEntity>("Bad VO."));
        Result<int> validVo = Result<int>.Success(42);

        // Act
        DomainGuard.CheckNotEmpty(Guid.Empty, "UserId", errors);
        string text = DomainGuard.CheckText("  ", 10, "Title", errors);
        string? opt = DomainGuard.CheckOptionalText("123456", 5, "Desc", errors);
        DomainGuard.CheckEnum((SampleKind)99, "Type", errors);
        DomainGuard.CheckNonNegative(-1, "Qty", errors);
        DomainGuard.CheckPositive(0, "Limit", errors);
        DomainGuard.CheckInRange(20, 1, 10, "Percent", errors);
        int collectedFail = DomainGuard.Collect(failedVo, errors);
        int collectedOk = DomainGuard.Collect(validVo, errors);
        Result<DummyEntity> failureResult = DomainGuard.ValidationFailure<DummyEntity>(errors);

        // Assert
        text.Should().BeEmpty();
        opt.Should().Be("123456");
        collectedFail.Should().Be(0);
        collectedOk.Should().Be(42);
        errors.Should().HaveCount(8);
        failureResult.IsFailure.Should().BeTrue();
        failureResult.Error.Code.Should().Be("DummyEntity.Invalid");
        failureResult.Error.Type.Should().Be(ErrorType.Validation);
        failureResult.Error.Message.Should().Contain("UserId cannot be empty.");
        failureResult.Error.Message.Should().Contain("Bad VO.");
    }
}
