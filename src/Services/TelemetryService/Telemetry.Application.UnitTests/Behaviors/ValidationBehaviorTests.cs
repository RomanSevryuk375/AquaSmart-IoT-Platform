using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Telemetry.Application.Behaviors;

namespace Telemetry.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestCommand : IRequest<Result>;
    public sealed record TestGenericCommand : IRequest<Result<string>>;

    private readonly RequestHandlerDelegate<Result> _nextMock;
    private readonly RequestHandlerDelegate<Result<string>> _nextGenericMock;

    public ValidationBehaviorTests()
    {
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
        _nextMock.Invoke().Returns(Result.Success());

        _nextGenericMock = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        _nextGenericMock.Invoke().Returns(Result<string>.Success("Success Value"));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenNoValidatorsExist_CallsNextDelegate()
    {
        // Arrange
        var request = new TestCommand();
        var validators = new List<IValidator<TestCommand>>();
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidatorsPass_CallsNextDelegate()
    {
        // Arrange
        var request = new TestCommand();
        IValidator<TestCommand> validatorMock = Substitute.For<IValidator<TestCommand>>();
        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var validators = new List<IValidator<TestCommand>> { validatorMock };
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _nextMock.Received(1).Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidationFails_ReturnsFailureResultWithoutCallingNext()
    {
        // Arrange
        var request = new TestCommand();
        IValidator<TestCommand> validatorMock = Substitute.For<IValidator<TestCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is invalid");
        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { validationFailure }));

        var validators = new List<IValidator<TestCommand>> { validatorMock };
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);

        // Act
        Result result = await behavior.Handle(request, _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Error");
        result.Error.Message.Should().Be("Value is invalid");
        await _nextMock.DidNotReceive().Invoke();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenValidationFailsForGenericResult_ReturnsFailureGenericResultWithoutCallingNext()
    {
        // Arrange
        var request = new TestGenericCommand();
        IValidator<TestGenericCommand> validatorMock = Substitute.For<IValidator<TestGenericCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is too short");
        validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestGenericCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { validationFailure }));

        var validators = new List<IValidator<TestGenericCommand>> { validatorMock };
        var behavior = new ValidationBehavior<TestGenericCommand, Result<string>>(validators);

        // Act
        Result<string> result = await behavior.Handle(request, _nextGenericMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Error");
        result.Error.Message.Should().Be("Value is too short");
        await _nextGenericMock.DidNotReceive().Invoke();
    }
}
