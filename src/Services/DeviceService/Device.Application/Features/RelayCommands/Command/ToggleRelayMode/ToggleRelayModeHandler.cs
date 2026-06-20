using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

internal sealed class ToggleRelayModeHandler(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleRelayModeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleRelayModeCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result<bool>.Failure(Error.NotFound<Relay>(
                $"{nameof(Relay)} {request.RelayId} not found"));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<bool>.Failure(ownership.Error);
        }

        existingRelay.SetMode(!existingRelay.IsManual);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(existingRelay.IsManual);
    }
}
