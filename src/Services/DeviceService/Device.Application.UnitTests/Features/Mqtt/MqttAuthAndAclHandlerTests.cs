using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Features.Mqtt;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using static Device.Application.Features.Mqtt.AuthenticateMqttClientHandler;

namespace Device.Application.UnitTests.Features.Mqtt;

public class MqttAuthAndAclHandlerTests
{
    private const string WorkerUsername = "device-service-worker";
    private const string WorkerPassword = "worker-super-secret-password";
    private const string ValidToken = "ak_valid_test_token_123";
    private const string StoredHash = "hashed_token_value";
    private const string CanonicalMac = "AA:BB:CC:DD:EE:FF";
    private const string CompactMac = "AABBCCDDEEFF";

    private readonly IControllerRepository _controllerRepoMock = Substitute.For<IControllerRepository>();
    private readonly IDeviceTokenHasher _tokenHasherMock = Substitute.For<IDeviceTokenHasher>();
    private readonly IFusionCache _cacheMock = Substitute.For<IFusionCache>();
    private readonly ILogger<AuthenticateMqttClientHandler> _authLoggerMock = Substitute.For<ILogger<AuthenticateMqttClientHandler>>();
    private readonly ILogger<AuthorizeMqttActionHandler> _aclLoggerMock = Substitute.For<ILogger<AuthorizeMqttActionHandler>>();

    private readonly IOptions<MqttOptions> _mqttOptions = Options.Create(new MqttOptions
    {
        Username = WorkerUsername,
        Password = WorkerPassword
    });

    private AuthenticateMqttClientHandler CreateAuthHandler() =>
        new(_controllerRepoMock, _tokenHasherMock, _cacheMock, _mqttOptions, _authLoggerMock);

    private AuthorizeMqttActionHandler CreateAclHandler() =>
        new(_mqttOptions, _aclLoggerMock);

    private void SetupCacheReturns(ControllerMqttAuthCacheItem? item)
    {
        _cacheMock.GetOrSetAsync<ControllerMqttAuthCacheItem?>(
                Arg.Any<string>(),
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerMqttAuthCacheItem?>, CancellationToken, Task<ControllerMqttAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(item);
    }

    [Fact]
    public async Task AuthenticateMqttClient_WithValidControllerTokenAndClientId_ReturnsAllowNonSuperuser()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        SetupCacheReturns(new ControllerMqttAuthCacheItem(controllerId, StoredHash));
        _tokenHasherMock.Verify(ValidToken, StoredHash).Returns(true);

        var query = new AuthenticateMqttClientQuery
        {
            Username = CanonicalMac,
            Password = ValidToken,
            ClientId = $"aqua-esp32-{CompactMac}"
        };

        // Act
        Result<MqttAuthResult> result = await CreateAuthHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeTrue();
        result.Value.IsSuperuser.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateMqttClient_WithCompactMacUsername_NormalizesAndReturnsAllow()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        string expectedCacheKey = $"controller:mqtt-auth:{CanonicalMac}";
        _cacheMock.GetOrSetAsync<ControllerMqttAuthCacheItem?>(
                expectedCacheKey,
                Arg.Any<Func<FusionCacheFactoryExecutionContext<ControllerMqttAuthCacheItem?>, CancellationToken, Task<ControllerMqttAuthCacheItem?>>>(),
                Arg.Any<FusionCacheEntryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(new ControllerMqttAuthCacheItem(controllerId, StoredHash));
        _tokenHasherMock.Verify(ValidToken, StoredHash).Returns(true);

        var query = new AuthenticateMqttClientQuery
        {
            Username = "aabbccddeeff",
            Password = ValidToken,
            ClientId = $"aqua-esp32-{CompactMac}"
        };

        // Act
        Result<MqttAuthResult> result = await CreateAuthHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeTrue();
        result.Value.IsSuperuser.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateMqttClient_WithInvalidToken_ReturnsDeny()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        SetupCacheReturns(new ControllerMqttAuthCacheItem(controllerId, StoredHash));
        _tokenHasherMock.Verify("wrong-token", StoredHash).Returns(false);

        var query = new AuthenticateMqttClientQuery
        {
            Username = CanonicalMac,
            Password = "wrong-token",
            ClientId = $"aqua-esp32-{CompactMac}"
        };

        // Act
        Result<MqttAuthResult> result = await CreateAuthHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeFalse();
        result.Value.IsSuperuser.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateMqttClient_WithMismatchedClientId_ReturnsDeny()
    {
        // Arrange
        var query = new AuthenticateMqttClientQuery
        {
            Username = CanonicalMac,
            Password = ValidToken,
            ClientId = "aqua-esp32-112233445566"
        };

        // Act
        Result<MqttAuthResult> result = await CreateAuthHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeFalse();
        result.Value.IsSuperuser.Should().BeFalse();
        _tokenHasherMock.DidNotReceiveWithAnyArgs().Verify(default!, default!);
    }

    [Fact]
    public async Task AuthenticateMqttClient_WithWorkerCredentials_ReturnsAllowSuperuser()
    {
        // Arrange
        var query = new AuthenticateMqttClientQuery
        {
            Username = WorkerUsername,
            Password = WorkerPassword,
            ClientId = "aqua-device-service-1"
        };

        // Act
        Result<MqttAuthResult> result = await CreateAuthHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeTrue();
        result.Value.IsSuperuser.Should().BeTrue();
    }

    [Theory]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/telemetry")]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/commands/ack")]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/relays/state")]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/status")]
    public async Task AuthorizeMqttAction_DevicePublishToOwnAllowedTopics_ReturnsAllow(string topic)
    {
        // Arrange
        var query = new AuthorizeMqttActionQuery
        {
            Username = CanonicalMac,
            Topic = topic,
            Action = "publish"
        };

        // Act
        Result<MqttAclResult> result = await CreateAclHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeTrue();
    }

    [Theory]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/commands")]
    [InlineData("aquasmart/devices/AABBCCDDEEFF/config/notify")]
    public async Task AuthorizeMqttAction_DeviceSubscribeToOwnAllowedTopics_ReturnsAllow(string topic)
    {
        // Arrange
        var query = new AuthorizeMqttActionQuery
        {
            Username = "aabbccddeeff",
            Topic = topic,
            Action = "subscribe"
        };

        // Act
        Result<MqttAclResult> result = await CreateAclHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeTrue();
    }

    [Theory]
    [InlineData("publish", "aquasmart/devices/AABBCCDDEEFF/commands")]
    [InlineData("publish", "aquasmart/devices/AABBCCDDEEFF/config/notify")]
    [InlineData("publish", "aquasmart/devices/112233445566/telemetry")]
    [InlineData("subscribe", "aquasmart/devices/112233445566/commands")]
    [InlineData("subscribe", "aquasmart/devices/AABBCCDDEEFF/telemetry")]
    public async Task AuthorizeMqttAction_DevicePublishToCommandsOrOtherMac_ReturnsDeny(
        string action,
        string topic)
    {
        // Arrange
        var query = new AuthorizeMqttActionQuery
        {
            Username = CanonicalMac,
            Topic = topic,
            Action = action
        };

        // Act
        Result<MqttAclResult> result = await CreateAclHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(MqttAclResult.Deny());
        _aclLoggerMock.ReceivedWithAnyArgs(1).Log(default, default, default!, default, default!);
    }

    [Theory]
    [InlineData("subscribe", "#")]
    [InlineData("subscribe", "aquasmart/devices/+/commands")]
    [InlineData("subscribe", "aquasmart/devices/AABBCCDDEEFF/#")]
    [InlineData("publish", "aquasmart/devices/+/telemetry")]
    public async Task AuthorizeMqttAction_DeviceSubscribeWithWildcardPlusOrHash_ReturnsDeny(
        string action,
        string topic)
    {
        // Arrange
        var query = new AuthorizeMqttActionQuery
        {
            Username = CompactMac,
            Topic = topic,
            Action = action
        };

        // Act
        Result<MqttAclResult> result = await CreateAclHandler().Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Allow.Should().BeFalse();
    }
}
