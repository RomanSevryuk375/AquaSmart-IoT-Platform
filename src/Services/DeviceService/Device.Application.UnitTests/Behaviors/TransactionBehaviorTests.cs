// Ignore Spelling: Rethrows

using Contracts.Abstractions;
using Contracts.Results;
using Device.Application.Behaviors;
using Device.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Device.Application.UnitTests.Behaviors;

public class TransactionBehaviorTests
{
    public sealed record TestQuery : IRequest<Result>;
    public sealed record TestCommand : IRequest<Result>, IBaseCommand;

    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly RequestHandlerDelegate<Result> _nextMock;

    public TransactionBehaviorTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _nextMock = Substitute.For<RequestHandlerDelegate<Result>>();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenRequestIsNotCommand_CallsNextWithoutTransaction()
    {
        // Arrange
        var behavior = new TransactionBehavior<TestQuery, Result>(_unitOfWorkMock);
        _nextMock.Invoke().Returns(Result.Success());

        // Act
        await behavior.Handle(new TestQuery(), _nextMock, CancellationToken.None);

        // Assert
        await _nextMock.Received(1).Invoke();
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCommandSucceeds_CommitsTransaction()
    {
        // Arrange
        var behavior = new TransactionBehavior<TestCommand, Result>(_unitOfWorkMock);
        _nextMock.Invoke().Returns(Result.Success());

        // Act
        Result result = await behavior.Handle(new TestCommand(), _nextMock, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Received.InOrder(() =>
        {
            _unitOfWorkMock.BeginTransactionAsync(Arg.Any<CancellationToken>());
            _nextMock.Invoke();
            _unitOfWorkMock.CommitTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenCommandFails_RollbacksTransaction()
    {
        // Arrange
        var behavior = new TransactionBehavior<TestCommand, Result>(_unitOfWorkMock);
        _nextMock.Invoke().Returns(Result.Failure(Error.Validation("Error", "Msg")));

        // Act
        Result result = await behavior.Handle(new TestCommand(), _nextMock, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        Received.InOrder(() =>
        {
            _unitOfWorkMock.BeginTransactionAsync(Arg.Any<CancellationToken>());
            _nextMock.Invoke();
            _unitOfWorkMock.RollbackTransactionAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public async Task Handle_WhenExceptionThrown_RollbacksTransactionAndRethrows()
    {
        // Arrange
        var behavior = new TransactionBehavior<TestCommand, Result>(_unitOfWorkMock);
        var exception = new Exception("Critical error");
        _nextMock.Invoke().ThrowsAsync(exception);

        // Act
        Func<Task> action = async () => await behavior.Handle(new TestCommand(), _nextMock, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Critical error");

        await _unitOfWorkMock.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}
