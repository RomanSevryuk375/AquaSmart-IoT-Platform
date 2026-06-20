using Device.Application.Interfaces;
using MassTransit;

namespace Device.Application.Features.Relays.Command.AddRelay;

internal class AddRelayHandler(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AddRelayCommand, Result<RelayCreatedResponse>>
{
    public async Task<Result<RelayCreatedResponse>> Handle(
        AddRelayCommand request,
        CancellationToken cancellationToken)
    {
        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<RelayCreatedResponse>.Failure(ownership.Error);
        }

        Result<Relay> relay = Relay.Create(
            id: NewId.NextGuid(),
            request.ControllerId, userContext.UserId, request.PowerSensorId,
            request.Name, request.ConnectionProtocol, request.ConnectionAddress,
            request.IsNormalyOpen, request.Purpose,
            request.IsActive, request.IsManual);
        if (relay.IsFailure)
        {
            return Result<RelayCreatedResponse>.Failure(relay.Error);
        }

        await relayRepository.AddAsync(relay.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RelayCreatedResponse>.Success(
            mapper.Map<RelayCreatedResponse>(relay.Value));
    }
}
