using System.Text.Json;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.UnitTests.Domain.Results;

public class ResultTests
{
    private sealed class SampleEntity
    {
        public string Name => nameof(SampleEntity);
    }

    [Fact]
    public void Success_InitializesIsSuccessAndErrorNone()
    {
        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_InitializesIsFailureAndError()
    {
        // Arrange
        Error error = Error.NotFound<SampleEntity>("Not found.");

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SampleEntity.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GenericResult_WhenFailureAndAccessingValue_ThrowsInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation<SampleEntity>("Invalid."));

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenericResult_ImplicitConversion_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 123;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(123);
    }

    [Fact]
    public void ErrorFactories_CreateExpectedCodesAndTypes()
    {
        // Assert
        Error.NotFound<SampleEntity>("m").Should().Be(new Error("SampleEntity.NotFound", "m", ErrorType.NotFound));
        Error.Validation<SampleEntity>("m").Should().Be(new Error("SampleEntity.Invalid", "m", ErrorType.Validation));
        Error.Conflict<SampleEntity>("m").Should().Be(new Error("SampleEntity.Conflict", "m", ErrorType.Conflict));
        Error.Failure<SampleEntity>("m").Should().Be(new Error("SampleEntity.Failure", "m", ErrorType.Failure));
        Error.Forbidden<SampleEntity>("m").Should().Be(new Error("SampleEntity.Forbidden", "m", ErrorType.Forbidden));
        Error.Unauthorized<SampleEntity>("m").Should().Be(new Error("SampleEntity.Unauthorized", "m", ErrorType.Unauthorized));
    }

    [Fact]
    public void JsonSerialization_RoundTripsResultAndGenericResult()
    {
        // Arrange
        Result success = Result.Success();
        Result failure = Result.Failure(Error.Conflict("Code.Conflict", "Conflict occurred"));
        Result<string> genericSuccess = Result<string>.Success("payload");
        Result<string> genericFailure = Result<string>.Failure(Error.Validation("Code.Invalid", "Bad input"));

        // Act
        Result? deserializedSuccess = JsonSerializer.Deserialize<Result>(JsonSerializer.Serialize(success));
        Result? deserializedFailure = JsonSerializer.Deserialize<Result>(JsonSerializer.Serialize(failure));
        Result<string>? deserializedGenericSuccess = JsonSerializer.Deserialize<Result<string>>(JsonSerializer.Serialize(genericSuccess));
        Result<string>? deserializedGenericFailure = JsonSerializer.Deserialize<Result<string>>(JsonSerializer.Serialize(genericFailure));

        // Assert
        deserializedSuccess.Should().NotBeNull();
        deserializedSuccess!.IsSuccess.Should().BeTrue();
        deserializedSuccess.Error.Should().Be(Error.None);

        deserializedFailure.Should().NotBeNull();
        deserializedFailure!.IsFailure.Should().BeTrue();
        deserializedFailure.Error.Should().Be(failure.Error);

        deserializedGenericSuccess.Should().NotBeNull();
        deserializedGenericSuccess!.IsSuccess.Should().BeTrue();
        deserializedGenericSuccess.Value.Should().Be("payload");

        deserializedGenericFailure.Should().NotBeNull();
        deserializedGenericFailure!.IsFailure.Should().BeTrue();
        deserializedGenericFailure.Error.Should().Be(genericFailure.Error);
    }
}
