using BuildingBlocks.Domain.Results;
using MassTransit;

namespace Device.Application.Features.Relays.Command.AddRelay;

internal sealed class AddRelayHandler(
    IRelayRepository relayRepository,
    IMapper mapper) : IRequestHandler<AddRelayCommand, Result<RelayCreatedResponse>>
{
    public async Task<Result<RelayCreatedResponse>> Handle(
        AddRelayCommand request,
        CancellationToken cancellationToken)
    {
        Result<Relay> relay = Relay.Create(
            id: NewId.NextGuid(),
            request.ControllerId, request.UserId, request.PowerSensorId,
            request.Name, request.ConnectionProtocol, request.ConnectionAddress,
            request.IsNormallyOpen, request.Purpose,
            request.IsActive, request.IsManual);
        if (relay.IsFailure)
        {
            return Result<RelayCreatedResponse>.Failure(relay.Error);
        }

        await relayRepository.AddAsync(relay.Value, cancellationToken);

        return Result<RelayCreatedResponse>.Success(
            mapper.Map<RelayCreatedResponse>(relay.Value));
    }
}
