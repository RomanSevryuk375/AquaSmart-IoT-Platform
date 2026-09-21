using System.Data;
using System.Data.Common;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using FluentAssertions;
using MassTransit;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Notification.Application.InternalEvents;
using NSubstitute;

namespace Notification.Application.UnitTests.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;

public class ProcessUnpublishedNoticesHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactoryMock = Substitute.For<ISqlConnectionFactory>();
    private readonly DbCommand _dbCommandMock = Substitute.For<DbCommand>();
    private readonly TestDbTransaction _dbTransaction = new();
    private readonly TestDbConnection _dbConnection;
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();

    public ProcessUnpublishedNoticesHandlerTests()
    {
        _dbConnection = new TestDbConnection(_dbCommandMock, _dbTransaction);
        _sqlConnectionFactoryMock.CreateConnection().Returns(_dbConnection);
        DbParameterCollection dbParameterCollectionMock = Substitute.For<DbParameterCollection>();
        DbParameter dbParameterMock = Substitute.For<DbParameter>();
        _dbCommandMock.Parameters.Returns(dbParameterCollectionMock);
        _dbCommandMock.CreateParameter().Returns(dbParameterMock);
        dbParameterCollectionMock.Add(Arg.Any<object>()).Returns(0);
        _dbCommandMock.ExecuteNonQueryAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
    }

    private ProcessUnpublishedNoticesHandler CreateHandler() => new ProcessUnpublishedNoticesHandler(_sqlConnectionFactoryMock, _publishEndpointMock);

    private void SetupQueryResults(
        List<(Guid Id, Guid UserId, string Message)> notifications,
        List<(Guid Id, string Email, bool EmailEnable, bool TgEnable, long? TelegramChatId, bool IsNotifyEnabled)> users)
    {
        int callCount = 0;

        _dbCommandMock.ExecuteReaderAsync(Arg.Any<System.Data.CommandBehavior>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // First query: Notifications
                    string[] columns = new[] { "Id", "UserId", "Message" };
                    var types = new[] { typeof(Guid), typeof(Guid), typeof(string) };
                    var rows = notifications.Select(n => new object?[] { n.Id, n.UserId, n.Message }).ToList();
                    return Task.FromResult<DbDataReader>(new TestDataReader(columns, types, rows));
                }
                if (callCount == 2)
                {
                    // Second query: Users
                    string[] columns = new[] { "Id", "Email", "EmailEnable", "TgEnable", "TelegramChatId", "IsNotifyEnabled" };
                    var types = new[] { typeof(Guid), typeof(string), typeof(bool), typeof(bool), typeof(long?), typeof(bool) };
                    var rows = users.Select(u => new object?[] { u.Id, u.Email, u.EmailEnable, u.TgEnable, u.TelegramChatId, u.IsNotifyEnabled }).ToList();
                    return Task.FromResult<DbDataReader>(new TestDataReader(columns, types, rows));
                }
                // Subsequent queries: return empty notifications reader to terminate loop
                return Task.FromResult<DbDataReader>(new TestDataReader(new[] { "Id", "UserId", "Message" }, new[] { typeof(Guid), typeof(Guid), typeof(string) }, new List<object?[]>()));
            });
    }

    [Fact]
    public async Task Handle_WhenNoUnpublishedNotifications_ReturnsSuccess()
    {
        // Arrange
        SetupQueryResults([], []);
        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasBothChannelsEnabled_PublishesBothAndMarksAsPublished()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        string message = "Test Message";

        SetupQueryResults(
            [(notificationId, userId, message)],
            [(userId, "user@test.com", true, true, 123456789, true)]);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendTelegramCommand>(cmd =>
                cmd.NotificationId == notificationId &&
                cmd.ChatId == 123456789 &&
                cmd.Message == message),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.NotificationId == notificationId &&
                cmd.Email == "user@test.com" &&
                cmd.Message == message),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasOnlyTgEnabled_PublishesTgOnlyAndMarksAsPublished()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        string message = "Test Message";

        SetupQueryResults(
            [(notificationId, userId, message)],
            [(userId, "user@test.com", false, true, 123456789, true)]);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendTelegramCommand>(cmd =>
                cmd.NotificationId == notificationId &&
                cmd.ChatId == 123456789 &&
                cmd.Message == message),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserHasOnlyEmailEnabled_PublishesEmailOnlyAndMarksAsPublished()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        string message = "Test Message";

        SetupQueryResults(
            [(notificationId, userId, message)],
            [(userId, "user@test.com", true, false, null, true)]);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailCommand>(cmd =>
                cmd.NotificationId == notificationId &&
                cmd.Email == "user@test.com" &&
                cmd.Message == message),
            Arg.Any<CancellationToken>());

        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<SendTelegramCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_MarksNotificationAsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        SetupQueryResults(
            [(notificationId, userId, "Test Message")],
            []);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsNotifyEnabledIsFalse_MarksNotificationAsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        SetupQueryResults(
            [(notificationId, userId, "Test Message")],
            [(userId, "user@test.com", true, true, 123456789, false)]);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoProvidersAreEnabledForUser_MarksNotificationAsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        SetupQueryResults(
            [(notificationId, userId, "Test Message")],
            [(userId, "user@test.com", false, false, null, true)]);

        var handler = CreateHandler();
        var command = new ProcessUnpublishedNoticesCommand();

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publishEndpointMock.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    private sealed class TestDataReader(string[] columnNames, Type[] columnTypes, List<object?[]> rows) : DbDataReader
    {
        private int _currentIndex = -1;

        public override bool Read()
        {
            _currentIndex++;
            return _currentIndex < rows.Count;
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());

        public override bool NextResult() => false;
        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;
        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;

        public override int FieldCount => columnNames.Length;
        public override string GetName(int ordinal) => columnNames[ordinal];
        public override int GetOrdinal(string name) => Array.FindIndex(columnNames, c => c.Equals(name, StringComparison.OrdinalIgnoreCase));
        public override object GetValue(int ordinal) => rows[_currentIndex][ordinal] ?? DBNull.Value;
        public override bool IsDBNull(int ordinal) => rows[_currentIndex][ordinal] is null or DBNull;

        public override bool HasRows => rows.Count > 0;
        public override bool IsClosed => false;
        public override int RecordsAffected => 0;

        public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal));
        public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal));
        public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal));
        public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(GetValue(ordinal));
        public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal));
        public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));
        public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal));
        public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
        public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal));
        public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));
        public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));
        public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal))!;
        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
        public override Type GetFieldType(int ordinal) => columnTypes[ordinal];
        public override int GetValues(object[] values)
        {
            int count = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < count; i++)
            {
                values[i] = GetValue(i);
            }

            return count;
        }
        public override object this[int ordinal] => GetValue(ordinal);
        public override object this[string name] => GetValue(GetOrdinal(name));
        public override System.Collections.IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override int Depth => 0;
    }

    private sealed class TestDbTransaction : DbTransaction
    {
        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
        protected override DbConnection? DbConnection => null;
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public override void Commit() => CommitCount++;
        public override void Rollback() => RollbackCount++;
    }

    private sealed class TestDbConnection(DbCommand command, DbTransaction? transaction = null) : DbConnection
    {
        private readonly DbTransaction _transaction = transaction ?? new TestDbTransaction();

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "TestDb";
        public override ConnectionState State => ConnectionState.Open;
        public override string DataSource => "TestServer";
        public override string ServerVersion => "1.0";

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        public override Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected override DbCommand CreateDbCommand() => command;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _transaction;
    }
}

