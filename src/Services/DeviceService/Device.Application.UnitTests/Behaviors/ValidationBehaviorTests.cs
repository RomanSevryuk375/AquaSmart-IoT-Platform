// Ignore Spelling: Validators

using Contracts.Results;
using Device.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace Device.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest : IRequest<Result>;

    private readonly RequestHandlerDelegate<Result> _nextMock;

    public ValidationBehaviorTests()
    {
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithNoValidators_CallsNextAndReturnsSuccess()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([]);
        var request = new TestRequest();

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithValidRequest_CallsNextAndReturnsSuccess()
    {
        // Arrange
        IValidator<TestRequest> validatorMock = Substitute.For<IValidator<TestRequest>>();
        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, Result>([validatorMock]);
        var request = new TestRequest();

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WithInvalidRequest_ReturnsFailureAndShortCircuits()
    {
        // Arrange
        var validationFailure = new ValidationFailure("Property", "Invalid value");
        IValidator<TestRequest> validatorMock = Substitute.For<IValidator<TestRequest>>();

        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([validationFailure]));

        var behavior = new ValidationBehavior<TestRequest, Result>([validatorMock]);
        var request = new TestRequest();

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Error");
        result.Error.Message.Should().Be("Invalid value");

        await _nextMock.DidNotReceive().Invoke();
    }
}
