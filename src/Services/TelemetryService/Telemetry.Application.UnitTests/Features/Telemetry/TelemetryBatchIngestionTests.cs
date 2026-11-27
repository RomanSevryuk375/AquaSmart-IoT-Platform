using AutoMapper;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using MassTransit;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;
using Telemetry.Application.Features.Telemetry.Commands.IngestDeviceTelemetryBatch;
using Telemetry.Application.Interfaces;
using Telemetry.Application.MapProfiles;
using Telemetry.Application.Services;
using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Messaging.TelemetryConsumers;

namespace Telemetry.Application.UnitTests.Features.Telemetry;

public class TelemetryBatchIngestionTests
{
    private readonly ITelemetryRawDataRepository _telemetryRepositoryMock = Substitute.For<ITelemetryRawDataRepository>();
    private readonly ISensorRepository _sensorRepositoryMock = Substitute.For<ISensorRepository>();
    private readonly IEcosystemRepository _ecosystemRepositoryMock = Substitute.For<IEcosystemRepository>();
    private readonly IDeviceTokenValidator _deviceTokenValidatorMock = Substitute.For<IDeviceTokenValidator>();

    [Fact]
    public async Task TelemetryBatchReceivedFromDeviceConsumer_UsesSharedIngestionService_AndDoesNotCallDeviceTokenValidator()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).WithControllerId(controllerId).Build();

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);
        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns([sensor]);
        _telemetryRepositoryMock.GetByExternalMessageIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RawTelemetry?)null);

        var ingestionService = new TelemetryBatchIngestionService(
            _telemetryRepositoryMock,
            _sensorRepositoryMock,
            _ecosystemRepositoryMock);
        var handler = new IngestDeviceTelemetryBatchHandler(ingestionService);

        ISender senderMock = Substitute.For<ISender>();
        senderMock.Send(Arg.Any<IngestDeviceTelemetryBatchCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => handler.Handle(
                callInfo.Arg<IngestDeviceTelemetryBatchCommand>(),
                callInfo.Arg<CancellationToken>()));

        var loggerFactory = Substitute.For<Microsoft.Extensions.Logging.ILoggerFactory>();
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TelemetryRawProfile>(), loggerFactory);
        IMapper mapper = mapperConfig.CreateMapper();

        var consumer = new TelemetryBatchReceivedFromDeviceConsumer(senderMock, mapper);
        var integrationEvent = new TelemetryBatchReceivedFromDeviceEvent
        {
            ControllerId = controllerId,
            UserId = userId,
            MacAddress = "AA:BB:CC:DD:EE:FF",
            Items =
            [
                new TelemetryBatchEventItem
                {
                    SensorId = sensor.Id,
                    Value = 26.4,
                    ExternalMessageId = "AABBCCDDEEFF-1-1-0",
                    RecordedAt = DateTime.UtcNow
                }
            ]
        };

        ConsumeContext<TelemetryBatchReceivedFromDeviceEvent> context = Substitute.For<ConsumeContext<TelemetryBatchReceivedFromDeviceEvent>>();
        context.Message.Returns(integrationEvent);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await _deviceTokenValidatorMock.DidNotReceiveWithAnyArgs()
            .ValidateAsync(default!, default!, default);
        await _telemetryRepositoryMock.Received(1)
            .AddAsync(Arg.Is<RawTelemetry>(r => r.ExternalMessageId == "AABBCCDDEEFF-1-1-0"), Arg.Any<CancellationToken>());
        await _telemetryRepositoryMock.Received(1)
            .SaveBatchWithDuplicateHandlingAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddTelemetryBatchHandler_ValidatesTokenAndDelegatesToSharedIngestionService()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        ITelemetryBatchIngestionService ingestionServiceMock = Substitute.For<ITelemetryBatchIngestionService>();
        var items = new List<TelemetryBatchEventItem>
        {
            new()
            {
                SensorId = Guid.NewGuid(),
                Value = 21.5,
                ExternalMessageId = "msg-http-1",
                RecordedAt = DateTime.UtcNow
            }
        };

        _deviceTokenValidatorMock.ValidateAsync("AA:BB:CC:DD:EE:FF", "ak_valid", Arg.Any<CancellationToken>())
            .Returns(Result<ValidateResponseDto>.Success(new ValidateResponseDto
            {
                ControllerId = controllerId,
                UserId = userId
            }));
        ingestionServiceMock.IngestAsync(controllerId, userId, items, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new AddTelemetryBatchHandler(_deviceTokenValidatorMock, ingestionServiceMock);
        var command = new AddTelemetryBatchCommand
        {
            MacAddress = "AA:BB:CC:DD:EE:FF",
            DeviceToken = "ak_valid",
            Items = items
        };

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _deviceTokenValidatorMock.Received(1).ValidateAsync("AA:BB:CC:DD:EE:FF", "ak_valid", Arg.Any<CancellationToken>());
        await ingestionServiceMock.Received(1).IngestAsync(controllerId, userId, items, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TelemetryBatchIngestionService_WithDuplicateExternalMessageIdsInSameBatch_DeduplicatesBeforeDbLookup()
    {
        // Arrange
        var controllerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Ecosystem ecosystem = new EcosystemBuilder().WithControllerId(controllerId).WithUserId(userId).Build();
        Sensor sensor = new SensorBuilder().WithEcosystemId(ecosystem.Id).WithControllerId(controllerId).Build();

        _ecosystemRepositoryMock.GetByControllerIdAsync(controllerId, Arg.Any<CancellationToken>())
            .Returns(ecosystem);
        _sensorRepositoryMock.GetAllByEcosystemId(ecosystem.Id, Arg.Any<CancellationToken>())
            .Returns([sensor]);
        _telemetryRepositoryMock.GetByExternalMessageIdAsync("dup-msg-1", Arg.Any<CancellationToken>())
            .Returns((RawTelemetry?)null);

        var service = new TelemetryBatchIngestionService(
            _telemetryRepositoryMock,
            _sensorRepositoryMock,
            _ecosystemRepositoryMock);

        var items = new List<TelemetryBatchEventItem>
        {
            new() { SensorId = sensor.Id, Value = 23.1, ExternalMessageId = "dup-msg-1", RecordedAt = DateTime.UtcNow },
            new() { SensorId = sensor.Id, Value = 23.2, ExternalMessageId = "dup-msg-1", RecordedAt = DateTime.UtcNow }
        };

        // Act
        Result result = await service.IngestAsync(controllerId, userId, items, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _telemetryRepositoryMock.Received(1)
            .GetByExternalMessageIdAsync("dup-msg-1", Arg.Any<CancellationToken>());
        await _telemetryRepositoryMock.Received(1)
            .AddAsync(Arg.Is<RawTelemetry>(r => r.ExternalMessageId == "dup-msg-1"), Arg.Any<CancellationToken>());
        await _telemetryRepositoryMock.Received(1)
            .SaveBatchWithDuplicateHandlingAsync(Arg.Any<CancellationToken>());
    }
}
